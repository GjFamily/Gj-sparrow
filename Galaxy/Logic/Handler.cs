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

        private int nextSendTickCount = 0;

        private int nextSendTickCountOnSerialize = 0;

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
            if (!PeerClient.connected)
            {
                //PeerClient.Wait();
                return;
            }

            // the messageQueue might be paused. in that case a thread will send acknowledgements only. nothing else to do here.
            //if (PeerClient.isMessageQueueRunning)
            //{
            //    return;
            //}

            bool doDispatch = true;
            while (PeerClient.isMessageQueueRunning && doDispatch)
            {
                // DispatchIncomingCommands() returns true of it found any command to dispatch (event, result or state change)
                Profiler.BeginSample("DispatchIncomingCommands");
                doDispatch = PeerClient.DispatchIncomingCommands();
                Profiler.EndSample();
            }

            int currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);  // avoiding Environment.TickCount, which could be negative on long-running platforms
            if (PeerClient.isMessageQueueRunning && currentMsSinceStart > this.nextSendTickCountOnSerialize)
            {
                PeerClient.Update();
                this.nextSendTickCountOnSerialize = currentMsSinceStart + this.updateIntervalOnSerialize;
                this.nextSendTickCount = 0;     // immediately send when synchronization code was running
            }

            currentMsSinceStart = (int)(Time.realtimeSinceStartup * 1000);
            if (currentMsSinceStart > this.nextSendTickCount)
            {
                bool doSend = true;
                while (PeerClient.isMessageQueueRunning && doSend)
                {
                    // Send all outgoing commands
                    Profiler.BeginSample("SendOutgoingCommands");
                    doSend = PeerClient.SendOutgoingCommands();
                    Profiler.EndSample();
                }

                this.nextSendTickCount = currentMsSinceStart + this.updateInterval;
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

                if (!PeerClient.isMessageQueueRunning && (PeerClient.LocalTimestamp - PeerClient.LastTimestamp) > 10 * 1000)
                {
                    //UnityEngine.Debug.Log(PeerClient.LocalTimestamp);
                    //UnityEngine.Debug.Log(PeerClient.LastTimestamp);
                    PeerClient.Ping();
                }
            }

            return sendThreadShouldRun;
        }

    }

}