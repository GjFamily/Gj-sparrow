using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

using UnityEngine.Profiling;

// 
namespace Gj.Galaxy.Logic{
    internal class Handler : MonoBehaviour
    {
        public static Handler SP;

        public int updateInterval;  // time [ms] between consecutive SendOutgoingCommands calls

        public int updateIntervalOnSerialize;  // time [ms] between consecutive RunViewUpdate calls (sending syncs, etc)

        public int updateStatInterval;

        private int nextSendTickCount = 0;

        private int nextSendTickCountOnSerialize = 0;

        private int nextStatCount = 0;

        private static bool sendThreadShouldRun;

        private static Stopwatch timerToStopConnectionInBackground;

        protected internal static bool AppQuits;

        protected internal static Type PingImplementation = null;

        protected void Awake()
        {
            if (SP != null && SP != this && SP.gameObject != null)
            {
                GameObject.DestroyImmediate(SP.gameObject);
            }

            SP = this;
            DontDestroyOnLoad(this.gameObject);

            this.updateInterval = 1000 / PeerClient.sendRate;
            this.updateIntervalOnSerialize = 1000 / PeerClient.sendRateOnSerialize;
            this.updateStatInterval = 1000 / PeerClient.statRate;

            Handler.StartFallbackSendAckThread();
        }

        protected void Start()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                //GameConnect.NewSceneLoaded();
                //GameConnect.SetLevelPrefix(short.Parse(SceneManagerHelper.ActiveSceneBuildIndex.ToString()));
            };
        }

        protected void OnApplicationQuit()
        {
            Handler.AppQuits = true;
            StopFallbackSendAckThread();
            PeerClient.Disconnect();
        }

        protected void OnApplicationPause(bool pause)
        {
            if (PeerClient.BackgroundTimeout > 0.1f)
            {
                if (timerToStopConnectionInBackground == null)
                {
                    timerToStopConnectionInBackground = new Stopwatch();
                }
                timerToStopConnectionInBackground.Reset();

                if (pause)
                {
                    timerToStopConnectionInBackground.Start();
                }
                else
                {
                    timerToStopConnectionInBackground.Stop();
                }
            }
        }

        protected void OnDestroy()
        {
            Handler.StopFallbackSendAckThread();
        }

        protected void Update()
        {
            if (PeerClient.offlineMode) return;

            // 提供client内部更新
            PeerClient.Refresh();

            if (!PeerClient.connected)
            {
                //PeerClient.Wait();
                return;
            }

			// DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
            Profiler.BeginSample("DispatchIncomingCommands");
            PeerClient.DispatchIncomingCommands();
            Profiler.EndSample();

            int currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);  // avoiding Environment.TickCount, which could be negative on long-running platforms
            if (currentMsSinceStart > this.nextSendTickCountOnSerialize)
            {
                PeerClient.Update();
                this.nextSendTickCountOnSerialize = currentMsSinceStart + this.updateIntervalOnSerialize;
                this.nextSendTickCount = 0;     // immediately send when synchronization code was running
            }

            currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);
            if (currentMsSinceStart > this.nextSendTickCount)
            {
				// Send all outgoing commands
                Profiler.BeginSample("SendOutgoingCommands");
                PeerClient.SendOutgoingCommands();
                Profiler.EndSample();

                this.nextSendTickCount = currentMsSinceStart + this.updateInterval;
            }

            currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);
            if(currentMsSinceStart > this.nextStatCount)
            {
                PeerClient.Stat();
                this.nextStatCount = currentMsSinceStart + this.updateStatInterval;
            }
        }

        public static void StartFallbackSendAckThread()
        {
#if !UNITY_WEBGL
            if (sendThreadShouldRun)
            {
                return;
            }

            sendThreadShouldRun = true;
            SP.StartCoroutine(StartBackgroundCalls());
#endif
        }

        public static void StopFallbackSendAckThread()
        {
#if !UNITY_WEBGL
            sendThreadShouldRun = false;
#endif
        }

        public static IEnumerator StartBackgroundCalls(){

            while (true)
            {
                yield return new WaitForSecondsRealtime(1f);
                if(!FallbackSendAckThread()){
                    break;
                }
            }
            sendThreadShouldRun = false;
        }

        /// <summary>A thread which runs independent from the Update() calls. Keeps connections online while loading or in background. See PhotonNetwork.BackgroundTimeout.</summary>
        public static bool FallbackSendAckThread()
        {
            if (sendThreadShouldRun && !PeerClient.offlineMode)
            {
                // check if the client should disconnect after some seconds in background
                if (timerToStopConnectionInBackground != null && PeerClient.BackgroundTimeout > 0.1f)
                {
                    if (timerToStopConnectionInBackground.ElapsedMilliseconds > PeerClient.BackgroundTimeout * 1000)
                    {
                        if (PeerClient.connected)
                        {
                            PeerClient.Disconnect();
                        }
                        timerToStopConnectionInBackground.Stop();
                        timerToStopConnectionInBackground.Reset();
                        return sendThreadShouldRun;
                    }
                }

                if ((PeerClient.LocalTimestamp - PeerClient.LastPingTimestamp) > PeerClient.pingInterval)
                {
                    PeerClient.Ping();
                }
            }

            return sendThreadShouldRun;
        }

    }

}