using System.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using Gj.Galaxy.Network;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif


namespace Gj.Galaxy.Logic{
    public static class PeerClient
    {
        public const string version = "1.1";

        internal static readonly Handler back;

        private static Client client = new Client();
        public static NetworkListener Listener = new NetworkListener();

        internal const string serverSettingsAssetFile = "GalaxySettings";

        public static ServerSettings ServerSettings = (ServerSettings)Resources.Load(PeerClient.serverSettingsAssetFile, typeof(ServerSettings));

        public static string ServerAddress { get { return (client != null) ? GetServerAddress() : "<not connected>"; } }

        //public static bool InstantiateInRoomOnly = true;
        public static bool UseRpcMonoBehaviourCache = false;

        public static LogLevel logLevel = LogLevel.Error;

        public static float precisionForVectorSynchronization = 0.000099f;

        public static float precisionForQuaternionSynchronization = 1.0f;

        public static float precisionForFloatSynchronization = 0.01f;

        public static bool UsePrefabCache = true;

        public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

        public static HashSet<GameObject> SendMonoMessageTargets;

        public static Type SendMonoMessageTargetType = typeof(MonoBehaviour);

        public static bool StartRpcsAsCoroutine = true;

        private static bool isOfflineMode = false;
        private static bool isConnect = false;

        private static int sendInterval = 50; // in miliseconds.

        private static int sendIntervalOnSerialize = 100; // in miliseconds. I.e. 100 = 100ms which makes 10 times/second

        public static bool connected
        {
            get
            {
                if (offlineMode)
                {
                    return true;
                }

                if (client == null)
                {
                    return false;
                }

                return client.IsConnected;
            }
        }


        public static bool offlineMode
        {
            get
            {
                return isOfflineMode;
            }

            set
            {
                if (value == isOfflineMode)
                {
                    return;
                }

                if (value && connected)
                {
                    Debug.LogError("Can't start OFFLINE mode while connected!");
                    return;
                }

                if (client.State != Network.ConnectionState.Disconnected)
                {
                    client.Disconnect(); // Cleanup (also calls OnLeftRoom to reset stuff)
                }
                isOfflineMode = value;
            }
        }

        public static int sendRate
        {
            get
            {
                return 1000 / sendInterval;
            }

            set
            {
                sendInterval = 1000 / value;
                if (back != null)
                {
                    back.updateInterval = sendInterval;
                }

                if (value < sendRateOnSerialize)
                {
                    // sendRateOnSerialize needs to be <= sendRate
                    sendRateOnSerialize = value;
                }
            }
        }

        public static int sendRateOnSerialize
        {
            get
            {
                return 1000 / sendIntervalOnSerialize;
            }

            set
            {
                if (value > sendRate)
                {
                    Debug.LogError("Error: Can not set the OnSerialize rate higher than the overall SendRate.");
                    value = sendRate;
                }

                sendIntervalOnSerialize = 1000 / value;
                if (back != null)
                {
                    back.updateIntervalOnSerialize = sendIntervalOnSerialize;
                }
            }
        }

        // 有队列namespace在运行：game同步
        // 用来触发game.update,发送队列消息，获取队列消息
        public static bool isMessageQueueRunning
        {
            get
            {
                return m_isMessageQueueRunning;
            }

            set
            {
                if (value) Handler.StartFallbackSendAckThread();
                m_isMessageQueueRunning = value;
            }
        }

        private static bool m_isMessageQueueRunning = false;

        //public static int unreliableCommandsLimit
        //{
        //    get
        //    {
        //        return client.LimitOfUnreliableCommands;
        //    }

        //    set
        //    {
        //        client.LimitOfUnreliableCommands = value;
        //    }
        //}

        //public static int ResentReliableCommands
        //{
        //    get { return client.ResentReliableCommands; }
        //}

        private static bool UsePreciseTimer = true;
        static Stopwatch startupStopwatch;

        public static float BackgroundTimeout = 60.0f;

        public static double time
        {
            get
            {
                uint u = (uint)ServerTimestamp;
                double t = u;
                return t / 1000;
            }
        }

        public static long ServerTimestamp
        {
            get
            {
                if (offlineMode)
                {
                    if (UsePreciseTimer && startupStopwatch != null && startupStopwatch.IsRunning)
                    {
                        return (int)startupStopwatch.ElapsedMilliseconds;
                    }
                    return Environment.TickCount;
                }

                return client.ServerTimestamp;
            }
        }

        public static long LocalTimestamp
        {
            get
            {
                return client.LocalTimestamp();
            }
        }

        public static long LastTimestamp
        {
            get
            {
                return client.LastTimestamp;
            }
        }

        //public static int MaxResendsBeforeDisconnect
        //{
        //    get { return client.SentCountAllowance; }
        //    set
        //    {
        //        if (value < 3) value = 3;
        //        if (value > 10) value = 10;
        //        client.SentCountAllowance = value;
        //    }
        //}

        //public static int QuickResends
        //{
        //    get { return client.QuickResendAttempts; }
        //    set
        //    {
        //        if (value < 0) value = 0;
        //        if (value > 3) value = 3;
        //        client.QuickResendAttempts = (byte)value;
        //    }
        //}
        //private static AuthConnect mAuth;
        //public static AuthConnect Auth
        //{
        //    get
        //    {
        //        if (mAuth != null) return mAuth;
        //        var n = client.Of((byte)NamespaceId.Auth);
        //        mAuth = new AuthConnect(n);
        //        return mAuth;
        //    }
        //}
        //private static ChatConnect mChat;
        //public static ChatConnect Chat
        //{
        //    get
        //    {
        //        if (mChat != null) return mChat;
        //        var n = client.Of((byte)NamespaceId.Chat);
        //        mChat = new ChatConnect(n);
        //        return mChat;
        //    }
        //}
        //private static SceneConnect mScene;
        //public static SceneConnect Scene
        //{
        //    get
        //    {
        //        if (mScene != null) return mScene;
        //        var n = client.Of((byte)NamespaceId.Scene);
        //        mScene = new SceneConnect(n);
        //        return mScene;
        //    }
        //}


        //private static GameConnect mGame;
        //public static GameConnect Game
        //{
        //    get
        //    {
        //        if (mGame != null) return mGame;
        //        var n = client.Of((byte)NamespaceId.Scene);
        //        n = n.Of((byte)SceneRoom.Game);
        //        mGame = new GameConnect(n);
        //        return mGame;
        //    }
        //}

        //private static TeamConnect mTeam;
        //public static TeamConnect Team
        //{
        //    get
        //    {
        //        if (mTeam != null) return mTeam;
        //        var n = client.Of((byte)NamespaceId.Scene);
        //        n = n.Of((byte)SceneRoom.Team);
        //        mTeam = new TeamConnect(n);
        //        return mTeam;
        //    }
        //}

        public delegate void EventCallback(byte eventCode, object content, int senderId);

        public static EventCallback OnEventCall;

        static PeerClient()
        {
#if UNITY_EDITOR

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log(string.Format("Not playing {0} {1}", UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode));
                return;
            }
            Debug.Log("PeerClient");

            InternalCleanPhotonMonoFromSceneIfStuck();
#endif

            //if (ServerSettings != null)
            //{
            //    Application.runInBackground = ServerSettings.RunInBackground;
            //}

            // Set up a MonoBehaviour to run Photon, and hide it
            GameObject GO = new GameObject();
            back = (Handler)GO.AddComponent<Handler>();
            GO.name = "GalaxyBack";
            GO.hideFlags = HideFlags.HideInHierarchy;

            client.ReconnectTimes = 5;
            //client.SentCountAllowance = 7;


#if UNITY_XBOXONE
            client.AuthMode = AuthModeOption.Auth;
#endif

            if (UsePreciseTimer)
            {
                startupStopwatch = new Stopwatch();
                startupStopwatch.Start();
                client.LocalTimestamp = () => (long)startupStopwatch.ElapsedMilliseconds;
            }
        }

        public static Namespace Of(byte ns){
            return PeerClient.client.Of(ns);
        }

        public static Namespace Of(NamespaceId ns){
            return PeerClient.client.Of((byte)ns);
        }

        /// <summary>
        /// Gets the NameServer Address (with prefix and port), based on the set protocol (this.UsedProtocol).
        /// </summary>
        /// <returns>NameServer Address (with prefix and port).</returns>
        private static string GetServerAddress()
        {
            if (ServerSettings == null) return "";
            string protocolPrefix = string.Empty;
            string result = string.Format("ws://{0}", ServerSettings.ServerAddress);

            return result;
        }

        public static bool Connect()
        {
            if (client.State != Network.ConnectionState.Disconnected)
            {
                Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + client.State);
                return false;
            }
            if (ServerSettings == null)
            {
                Debug.LogError("Can't connect: Loading settings failed. ServerSettings asset must be in any 'Resources' folder as: " + serverSettingsAssetFile);
                return false;
            }
            if (ServerSettings.HostType == ServerSettings.HostingOption.NotSet)
            {
                Debug.LogError("You did not select a Hosting Type in your ServerSettings. Please set it up or don't use ConnectUsingSettings().");
                return false;
            }

            // only apply Settings if logLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
            if (PeerClient.logLevel == LogLevel.Error)
            {
                PeerClient.logLevel = ServerSettings.Logging;
            }

            // only apply Settings if logLevel is default ( see ServerSettings.cs), else it means it's been set programmatically
            if (PeerClient.client.logLevel == LogLevel.Error)
            {
                PeerClient.client.logLevel = ServerSettings.NetworkLogging;
            }

            client.SetApp(ServerSettings.AppId, ServerSettings.Version, ServerSettings.Secret);

            if (ServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
            {
                offlineMode = true;
                return true;
            }

            offlineMode = false;
            client.listener = Listener;
            return client.Connect(GetServerAddress());
        }
        public static void Close(){
            if (client.IsRuning)
                client.Close();
        }

        //public static bool Reconnect()
        //{
        //    if (string.IsNullOrEmpty(ServerAddress))
        //    {
        //        Debug.LogWarning("Reconnect() failed. It seems the client wasn't connected before?! Current state: " + client.State);
        //        return false;
        //    }

        //    if (client.State != Network.ConnectionState.Disconnected)
        //    {
        //        Debug.LogWarning("Reconnect() failed. Can only connect while in state 'Disconnected'. Current state: " + client.State);
        //        return false;
        //    }

        //    if (offlineMode)
        //    {
        //        offlineMode = false; // Cleanup offline mode
        //        Debug.LogWarning("Reconnect() disabled the offline mode. No longer offline.");
        //    }

        //    if (!isMessageQueueRunning)
        //    {
        //        isMessageQueueRunning = true;
        //        Debug.LogWarning("Reconnect() enabled isMessageQueueRunning. Needs to be able to dispatch incoming messages.");
        //    }

        //    return client.Reconnect();
        //}

        public static void Disconnect()
        {
            if (offlineMode)
            {
                offlineMode = false;
                return;
            }

            if (client == null)
            {
                return; // Surpress error when quitting playmode in the editor
            }
            if(client.IsRuning)
                client.Disconnect();
        }

        public static int GetPing()
        {
            return client.PingTime;
        }

        public static void Update(){
            //Debug.Log("update");
            GameConnect.Update();
        }

        //public static void Wait(){
        //    if (client.IsRuning)
        //        client.WaitConnect();
        //}

        public static bool DispatchIncomingCommands(){
            return client.ReadQueue(10);
        }

        public static bool SendOutgoingCommands(){
            return client.WriteQueue(10);
        }

        public static void Ping()
        {
            if (client.IsConnected)
                client.Ping();
            else if(client.IsRuning)
                client.Reconnect();
        }

        public static HashSet<GameObject> FindGameObjectsWithComponent(Type type)
        {
            HashSet<GameObject> objectsWithComponent = new HashSet<GameObject>();

            Component[] targetComponents = (Component[])GameObject.FindObjectsOfType(type);
            for (int index = 0; index < targetComponents.Length; index++)
            {
                if (targetComponents[index] != null)
                {
                    objectsWithComponent.Add(targetComponents[index].gameObject);
                }
            }

            return objectsWithComponent;
        }


#if UNITY_EDITOR

        public static string FindAssetPath(string asset)
        {
            string[] guids = AssetDatabase.FindAssets(asset, null);
            if (guids.Length != 1)
            {
                return string.Empty;
            }
            else
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }
        }

        /// <summary>
        /// Gets the parent directory of a path. Recursive Function, will return null if parentName not found
        /// </summary>
        /// <returns>The parent directory</returns>
        /// <param name="path">Path.</param>
        /// <param name="parentName">Parent name.</param>
        public static string GetParent(string path, string parentName)
        {
            var dir = new DirectoryInfo(path);

            if (dir.Parent == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(parentName))
            {
                return dir.Parent.FullName;
            }

            if (dir.Parent.Name == parentName)
            {
                return dir.Parent.FullName;
            }

            return GetParent(dir.Parent.FullName, parentName);
        }
#endif

#if UNITY_EDITOR

        public static void InternalCleanPhotonMonoFromSceneIfStuck()
        {
            Handler[] Handlers = GameObject.FindObjectsOfType(typeof(Handler)) as Handler[];
            if (Handlers != null && Handlers.Length > 0)
            {
                foreach (Handler Handler in Handlers)
                {
                    Handler.gameObject.hideFlags = 0;

                    if (Handler.gameObject != null && Handler.gameObject.name == "GalaxyBack")
                    {
                        GameObject.DestroyImmediate(Handler.gameObject);
                    }

                    Component.DestroyImmediate(Handler);
                }
            }
        }
#endif
    }
}
