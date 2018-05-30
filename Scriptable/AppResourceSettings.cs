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

[CreateAssetMenu(fileName = "AppResourceSettings", menuName = "Gj/AppResourceSettings")]
public class AppResourceSettings : ScriptableObject
{
    [Tooltip("本地Manifest文件名称")]
    public string Manifest = "manifest.json";

    [Tooltip("App Info Url：为空不检查更新")]
    public string AppInfoUrl = "";

    [Tooltip("是否启用AssetBundle")]
    public bool AssetBundle = true;

    [Tooltip("Bundle存储路径:{version}->{1},{build}->{iOS},{time}->{2017-08-1010:10:10} 为空不检查更新")]
    public string BundlePath = "";

    [Tooltip("共用bundleName：自动加载，为空不加载")]
    public string CommonBundleName = "";

    [Tooltip("自动下载所有更新Bundle")]
    public bool auto = false;

}

//#if UNITY_EDITOR
//    public class ExportAssetBundles : Editor
//    {

//        [MenuItem("Assets/CreateAssetBundle")]
//        static void OnCreateAssetBundle()
//        {
//          var settings = Resource.Settings;

//            BuildPipeline.BuildAssetBundles(settings.path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);

//            BuildPipeline.BuildAssetBundles(settings.path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);

//            //刷新编辑器
//            AssetDatabase.Refresh();
//            Debug.Log("AssetBundle打包完毕");
//        }
//    }
//#endif
