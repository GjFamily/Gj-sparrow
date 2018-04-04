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

        public static int pingInterval = 1000 * 10; // 10s

        private static int sendInterval = 50; // in miliseconds.

        private static int sendIntervalOnSerialize = 100; // in miliseconds. I.e. 100 = 100ms which makes 10 times/second

        private static long inDataLength = 0;
        private static long outDataLength = 0;
        private static long preInDataLength = 0;
        private static long preOutDataLength = 0;
        private static int statInterval = 1000;  // in miliseconds
        private static bool is_stat = false;

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

        public static bool connecting
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
                return client.IsRuning;
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

                if (value && connecting)
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

        public static int statRate
        {
            get
            {
                return 1000 / statInterval;
            }

            set
            {
                if (value > statInterval)
                {
                    Debug.LogError("Error: Can not set the Stat rate higher than the overall SendRate.");
                    value = statInterval;
                }

                statInterval = 1000 / value;
                if (back != null)
                {
                    back.updateStatInterval = statInterval;
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

        private static bool UsePreciseTimer = true;
        static Stopwatch startupStopwatch;

        public static float BackgroundTimeout = 60.0f;

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

        public static long PingTime
        {
            get
            {
                return client.PingTime;
            }
        }

        public static long LastPingTimestamp
        {
            get
            {
                return client.LastPingTimestamp;
            }
        }

        public static long LastTimestamp
        {
            get
            {
                return client.LastTimestamp;
            }
        }

        public delegate void EventCallback(byte eventCode, object content, int senderId);

        public static EventCallback OnEventCall;

        static PeerClient()
        {
#if UNITY_EDITOR

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //Debug.Log(string.Format("Not playing {0} {1}", UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode));
                return;
            }
            Debug.Log("PeerClient");

            InternalCleanMonoFromSceneIfStuck();
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

        public static void SetStatic(bool sw)
        {
            inDataLength = 0;
            outDataLength = 0;
            if(sw && !is_stat){
                PeerClient.client.InData += PeerClient.InDataStat;
                PeerClient.client.OutData += PeerClient.OutDataStat;
            }else if(!sw && is_stat){
                PeerClient.client.InData -= PeerClient.InDataStat;
                PeerClient.client.OutData -= PeerClient.OutDataStat;
            }
            is_stat = sw;
        }

        internal static void InDataStat(long length)
        {
            inDataLength += length;
            preInDataLength += length;
        }

        internal static void OutDataStat(long length)
        {
            outDataLength += length;
            preOutDataLength += length;
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

        public static void Reconnend(){
            
        }

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

        internal static void Update(){
            //Debug.Log("update");
            GameConnect.Update();
        }

        internal static void ClientUpdate(){
            client.Update();
        }

        internal static void Stat(){
            if (!is_stat) return;
            Debug.Log(String.Format("All: {0}, in: {1}, out: {2}", inDataLength + outDataLength, preInDataLength, preOutDataLength));
            preInDataLength = 0;
            preOutDataLength = 0;
        }

        internal static bool DispatchIncomingCommands(){
            return client.ReadQueue(10);
        }

        internal static bool SendOutgoingCommands(){
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

        public static void InternalCleanMonoFromSceneIfStuck()
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
