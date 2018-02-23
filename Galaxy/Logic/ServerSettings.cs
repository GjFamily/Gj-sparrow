using System;
using System.Collections.Generic;
using Gj.Galaxy.Network;
using UnityEngine;

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

        public List<string> RpcList = new List<string>();   // set by scripts and or via Inspector

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

}
