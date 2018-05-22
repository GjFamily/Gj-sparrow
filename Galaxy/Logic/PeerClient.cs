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
	[Serializable]
    public class ServerSettings : ScriptableObject
    {
        public enum HostingOption { NotSet = 0, OfflineMode = 1, OnlineMode = 4 }

        public string AppId = "";
        public string Version = "";
        public string Secret = "";

        public HostingOption HostType = HostingOption.NotSet;

        public string ServerAddress = "";

        public LogLevel Logging = LogLevel.Error;
        public LogLevel NetworkLogging = LogLevel.Error;

        public bool RunInBackground = true;

        [HideInInspector]
        public bool DisableAutoOpenWizard;

        public void SetAppInfo(string appId, string version, string secret)
        {
            this.HostType = HostingOption.OnlineMode;
            this.AppId = appId;
            this.Version = version;
            this.Secret = secret;
        }

        public override string ToString()
        {
            return "ServerSettings: " + HostType + " " + ServerAddress;
        }
    }

    public static class PeerClient
    {
        public const string version = "1.1";

        internal static readonly Handler back;
        
		private static Nebula GameClient = new Nebula();
		private static Comet CenterClient= new Comet("");
        public static NetworkListener Listener = new NetworkListener();

        internal const string serverSettingsAssetFile = "GalaxySettings";

        public static ServerSettings ServerSettings = (ServerSettings)Resources.Load(PeerClient.serverSettingsAssetFile, typeof(ServerSettings));

		public static string ServerAddress { get { return (GameClient != null) ? GetServerAddress() : "<not connected>"; } }

        //public static bool InstantiateInRoomOnly = true;
        public static bool UseRpcMonoBehaviourCache = false;

        public static LogLevel logLevel = LogLevel.Error;

        public static float precisionForVectorSynchronization = 0.0001f; // 0.000099f;

        public static float precisionForQuaternionSynchronization = 1.0f;

        public static float precisionForFloatSynchronization = 0.01f;

        public static bool UsePrefabCache = true;

        public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

        public static HashSet<GameObject> SendMonoMessageTargets;

        public static Type SendMonoMessageTargetType = typeof(MonoBehaviour);

        public static bool StartRpcsAsCoroutine = true;

        private static bool isOfflineMode = true;
        private static bool isConnect = false;

        public static int pingInterval = 1000 * 10; // 10s

        private static int sendInterval = 50; // in miliseconds.

        private static int sendIntervalOnSerialize = 60; // in miliseconds. I.e. 100 = 100ms which makes 10 times/second

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

				if (GameClient == null)
                {
                    return false;
                }

				return GameClient.IsConnected;
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

				if (GameClient == null)
                {
                    return false;
                }
				return GameClient.IsRuning;
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

				if (GameClient.State != Network.ConnectionState.Disconnected)
                {
					GameClient.Disconnect();
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

                return GameClient.ServerTimestamp;
            }
        }

        public static long LocalTimestamp
        {
            get
            {
                return GameClient.LocalTimestamp();
            }
        }

        public static long PingTime
        {
            get
            {
				return CenterClient.PingTime;
            }
        }

        public static long LastPingTimestamp
        {
            get
            {
				return CenterClient.LastPingTimestamp;
            }
        }

        public static long LastTimestamp
        {
            get
            {
				return CenterClient.LastTimestamp;
            }
        }

        public delegate void EventCallback(byte eventCode, object content, int senderId);

        public static EventCallback OnEventCall;

        static PeerClient()
        {
#if UNITY_EDITOR

            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            InternalCleanMonoFromSceneIfStuck();
#endif

            GameObject GO = new GameObject();
            back = (Handler)GO.AddComponent<Handler>();
            GO.name = "GalaxyBack";
            GO.hideFlags = HideFlags.HideInHierarchy;

			GameClient.ReconnectTimes = 5;
			CenterClient.ReconnectTimes = 5;
         
#if UNITY_XBOXONE
            client.AuthMode = AuthModeOption.Auth;
#endif

            if (UsePreciseTimer)
            {
                startupStopwatch = new Stopwatch();
                startupStopwatch.Start();
				GameClient.LocalTimestamp = () => (long)startupStopwatch.ElapsedMilliseconds;
				CenterClient.LocalTimestamp = () => (long)startupStopwatch.ElapsedMilliseconds;
            }
        }

        public static ServerSettings Generate()
        {
            var settings = ScriptableObject.CreateInstance<ServerSettings>();
            PeerClient.ServerSettings = settings;
            return settings;
        }

        public static Namespace Of(byte ns){
			return PeerClient.GameClient.Of(ns);
        }

        public static Namespace Of(NamespaceId ns){
			return PeerClient.GameClient.Of((byte)ns);
        }

		public static CometProxy Register(byte no, ServiceListener service){
			return CenterClient.Register(no, service);
		}

        public static void SetStatic(bool sw)
        {
            inDataLength = 0;
            outDataLength = 0;
            if(sw && !is_stat){
				PeerClient.CenterClient.InData += PeerClient.InDataStat;
				PeerClient.CenterClient.OutData += PeerClient.OutDataStat;
            }else if(!sw && is_stat){
				PeerClient.CenterClient.InData -= PeerClient.InDataStat;
				PeerClient.CenterClient.OutData -= PeerClient.OutDataStat;
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
			if (GameClient.State != Network.ConnectionState.Disconnected)
            {
				Debug.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: " + GameClient.State);
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
			if (PeerClient.GameClient.logLevel == LogLevel.Error)
            {
				PeerClient.GameClient.logLevel = ServerSettings.NetworkLogging;
            }

			GameClient.SetApp(ServerSettings.AppId, ServerSettings.Version, ServerSettings.Secret);
			CenterClient.SetApp(ServerSettings.AppId, ServerSettings.Version, ServerSettings.Secret);

            if (ServerSettings.HostType == ServerSettings.HostingOption.OfflineMode)
            {
                offlineMode = true;
                return true;
            }

            offlineMode = false;
			GameClient.listener = Listener;
			return GameClient.Connect(GetServerAddress());
        }

        public static void Close(){
			GameClient.Close();
			CenterClient.Close();
        }

        public static void Disconnect()
        {
            if (offlineMode)
            {
                offlineMode = false;
                return;
            }

			if (GameClient == null)
            {
                return; // Surpress error when quitting playmode in the editor
            }
			if(GameClient.IsRuning)
				GameClient.Disconnect();
        }

        public static void SetToken(string token)
		{
			CenterClient.SetToken(token);
		}

        public static void BindCenter(string url)
		{
			if (CenterClient == null)
			{
				Debug.LogError("Center need token first");
			}
			else            
			{
				CenterClient.SwitchConnect(url);
			}         
		}      

        internal static void Update(){
			//Debug.Log("update");
			if (!offlineMode)
				CenterClient.Update();
        }

		internal static void Refresh(){
			GameClient.Refresh();
			CenterClient.Refresh();
        }

        internal static void Stat(){
            if (!is_stat) return;
            Debug.Log(String.Format("All: {0}, in: {1}, out: {2}", inDataLength + outDataLength, preInDataLength, preOutDataLength));
            preInDataLength = 0;
            preOutDataLength = 0;
        }
        
        internal static void DispatchIncomingCommands(){
			while (GameClient.ReadQueue(10)) { }
			CenterClient.ReadQueue();
        }

        internal static void SendOutgoingCommands(){
			while(GameClient.WriteQueue(10)){}
            CenterClient.ReadQueue();         
        }

        public static void Ping()
        {
			if (CenterClient.IsConnected)
				CenterClient.Ping();
			//else if (GameClient.IsConnected)
				//GameClient.Ping();

			if (!CenterClient.IsConnected && CenterClient.IsRuning)
				CenterClient.Reconnect();
			if (!GameClient.IsRuning && GameClient.IsRuning)
                GameClient.Reconnect();
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
