using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;
using Gj.Galaxy.Network;

[CreateAssetMenu(fileName = "GalaxyServerSettings", menuName = "Gj/GalaxyServerSettings")]
public class GalaxyServerSettings : ScriptableObject
{
    public enum HostingOption { OfflineMode = 1, OnlineMode = 2 }

    public string AppId = "";
    public string Version = "";
    public string Secret = "";

	public HostingOption HostType = HostingOption.OfflineMode;

    public string ServerAddress = "";

	public bool RunInBackground = true;

	public LogLevel Logging = LogLevel.Error;
    public LogLevel NetworkLogging = LogLevel.Error;


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