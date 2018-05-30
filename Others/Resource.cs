using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using SimpleJSON;


namespace Gj
{   
	public class AppInfo
	{
		public int version;

		public string resourceVersion;

		public string iosUrl;

		public string androidUrl;
	}

	public class Resource : MonoBehaviour
	{      
		public static Dictionary<string, AudioClip> audioClipMap = new Dictionary<string, AudioClip>();
		public static Dictionary<string, Texture> textureMap = new Dictionary<string, Texture>();
        
		public static Dictionary<string, string> hashBundleMap = new Dictionary<string, string>();

		private static AssetBundle manifestBundle;
		public static Dictionary<string, AssetBundle> loadBundleMap = new Dictionary<string, AssetBundle>();
		public static Dictionary<int, List<string>> levelMap = new Dictionary<int, List<string>>();
		public static Dictionary<int, Dictionary<string, AssetBundle>> assetMap = new Dictionary<int, Dictionary<string, AssetBundle>>();
		public static int bundleLevel = 0;

        // 能否更新App
        internal static bool CanUpdateApp
		{
			get
			{
				return Settings.AppInfoUrl != "";
			}
		}

        // 能否更新Bundle
        internal static bool CanUpdateBundle
		{
			get
			{
				return Settings.BundlePath != "" && AssetBundleStatus;
			}
		}

        // 是否启用Bundle
		internal static bool AssetBundleStatus
		{
			get
			{
				return Settings.AssetBundle && assetBundleStatus;
			}
		}
		private static Hash128 hash;
		internal static Hash128 Hash
		{
			get
			{
				if (hash == null)
				{
					hash = Hash128.Parse(localInfo.resourceVersion);
				}
				return hash;
			}
		}
		private static AppResourceSettings settings;
		internal static AppResourceSettings Settings
		{
			get
			{
				if (settings == null)
					settings = LoadSettings();
#if UNITY_EDITOR
				// 本机bundle模式测试, 外部地址用stream代替
				if (assetBundleStatus && settings.AssetBundle)
				{
					settings.BundlePath = "file:///" + Application.streamingAssetsPath + "/";
				}
#endif
				return settings;
			}
		}
#if UNITY_IOS
		private static string Build = "iOS";
#elif UNITY_ANDROID
		private static string Build = "Android";
#elif UNITY_STANDALONE_OSX
		private static string Build = "StandaloneOSXUniversal";
#elif UNITY_EDITOR
		private static string Build = "StandaloneWindows64";
#endif

#if UNITY_EDITOR
		// 是否能用AssetBundle
		private static bool assetBundleStatus = false;

		private static AppInfo info = new AppInfo();
		private static AppInfo localInfo = new AppInfo();
#else
		private static bool assetBundleStatus = true;

		private static AppInfo info;
		private static AppInfo localInfo;
#endif
		private static IEnumerator LoadLocalAppInfo()
		{
			Debug.Log("LoadLocalAppInfo");
			if (localInfo == null && Settings.Manifest != "")
			{
    			// 判断app清单文件
    			var manifest = Path.Combine(Application.persistentDataPath, Settings.Manifest);
    			if (!File.Exists(manifest))
				{
                    // 拷贝Asset到Persistent
                    Debug.Log("Copy Asset to persistent manifest");
					var url = Path.GetFileNameWithoutExtension(Settings.Manifest);
					var w = Resources.Load<TextAsset>(url);
                    if (w == null)
					{
						Debug.LogError("Manifest not exist:" + url);
                        yield break;
					}
					var o = JSONNode.Parse(w.text);
					o.SaveToFile(manifest);
					yield return null;
                    // 拷贝stream到Persistent
                    //Debug.Log("Copy stream to persistent manifest");
    	//			var url = Path.Combine(Application.streamingAssetsPath, Settings.Manifest);
    	//			WWW w = new WWW(url);
     //               yield return w;
					//if (w.isDone)
					//{
					//	var o = JSONNode.Parse(w.text);
     //                   o.SaveToCompressedFile(manifest);
					//}
					//else
					//{
					//	Debug.LogError("Manifest not exist:" + url);
					//	yield break;
					//}
    			}

				// 读取Persistent
                Debug.Log("Load persistent manifest");
				var oo = JSONNode.LoadFromFile(manifest);
    			localInfo = new AppInfo();
    			localInfo.androidUrl = oo["android"];
    			localInfo.iosUrl = oo["ios"];
    			localInfo.version = oo["version"].AsInt;
				localInfo.resourceVersion = oo["resource"];
				Debug.Log("Manifest:" + oo.ToString());
            } 
		}

        private static void UpdateLocalAppInfo()
		{
			Debug.Log("UpdateLocalAppInfo");
			if (info == null || info.version <=0) 
			{
				return;
			}
			if (!CanUpdateApp)
			{
				return;
			}
			Debug.Log("Write to persistent manifest");
            var manifest = Path.Combine(Application.persistentDataPath, Settings.Manifest);
			var r = JSONNode.Parse("{}");
			r["android"] = info.androidUrl;
			r["ios"] = info.iosUrl;
			r["version"] = info.version;
			r["resource"] = info.resourceVersion;
			Debug.Log("Manifest:" + r.ToString());
			r.SaveToFile(manifest);
			localInfo = info;
			hash = Hash128.Parse(localInfo.resourceVersion);
		}

		// 加载版本信息: 是否要更新
		public static IEnumerator InitAppInfo(bool stream, Action<bool, bool> callback)
		{
			// 读取本地app文件
			yield return LoadLocalAppInfo();
			// 初始化本地bundle
			yield return InitBundle(stream);
			if (info == null && CanUpdateApp)
			{
				Debug.Log("Download app info:"+Settings.AppInfoUrl);
				WWW w = new WWW(Settings.AppInfoUrl);
				yield return w;
				if (w.isDone)
				{
					var o = JSONNode.Parse(w.text);
                    Debug.Log("app info:" + w.text);
					info = new AppInfo();
					info.androidUrl = o["android"];
					info.iosUrl = o["ios"];
					info.version = o["version"].AsInt;
					info.resourceVersion = o["resource"];
					bool appStatus = false;
					bool resourceStatus = false;
					if (localInfo.version != info.version)
					{
						Debug.Log("App update:" + localInfo.version + "->" + info.version);
						appStatus = true;
					}
					else if (localInfo.resourceVersion != info.resourceVersion)
					{
						Debug.Log("Resource update:" + localInfo.resourceVersion + "->" + info.resourceVersion);
						resourceStatus = true;
					}
					callback(appStatus, resourceStatus);
				}
				else
				{
					callback(false, false);
				}
			}
			else
			{
				callback(false, false);
			}
		}      
		public static AppInfo GetAppInfo()
		{
			return info;
		}
        public static AppInfo GetLocalAppInfo()
		{
			return localInfo;
		}
        // 更新
		public static IEnumerator Update(Action<int> callback)
		{
			yield break;
            Debug.Log("update");
            // 更新app文件
            UpdateLocalAppInfo();
			// 更新清单文件
			string uri = GetBundleUrl(Build, null);
			Debug.Log("Download Bundle Mainfest:"+uri);
			if (manifestBundle != null)
			{
				manifestBundle.Unload(true);
			}
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, Hash, 0);
            yield return request.Send();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle == null)
			{            
                Debug.LogError("SWeb Manifest not exist:" + uri);
				callback(0);
                yield break;
			}
			manifestBundle = bundle;

            var updateList = LoadAllBundleName(bundle);

			if (Settings.auto)
			{

			}
			else
			{
                callback(0);
			}
		}

		public static Texture GetTexture(string tag)
		{
			if (!textureMap.ContainsKey(tag))
			{
				textureMap.Add(tag, Resources.Load<Texture>("texture/" + tag));
			}
			return textureMap[tag];
		}

		public static AudioClip GetAudioClip(string tag)
		{
			if (!audioClipMap.ContainsKey(tag))
			{
				audioClipMap.Add(tag, Resources.Load<AudioClip>("audio/" + tag));
			}
			return audioClipMap[tag];
		}

		public static Dictionary<string, string> GetLanguage(string language)
		{
			TextAsset ta = Resources.Load<TextAsset>("language/" + language);
			string text = ta.text;
			Dictionary<string, string> dic = new Dictionary<string, string>();
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{
				if (line == null)
				{
					continue;
				}
				string[] keyAndValue = line.Split('=');
				dic.Add(keyAndValue[0], keyAndValue[1]);
			}

			return dic;
		}

		private static AppResourceSettings LoadSettings()
		{
			AppResourceSettings loadedSettings = Resources.Load<AppResourceSettings>("AppResourceSettings");
			if (loadedSettings == null)
			{
				loadedSettings = ScriptableObject.CreateInstance<AppResourceSettings>();
				Debug.LogError("ResourceSettings is not exist (Resources)");
			}
			return loadedSettings;
		}
		private static void LoadAllAssetsName(string bundleName, AssetBundle bundle)
		{
			var levelList = levelMap[bundleLevel];
            levelList.Add(bundleName);
			loadBundleMap.Add(bundleName, bundle);
			var allAssets = bundle.GetAllAssetNames();
			var assets = assetMap[bundleLevel];
			var e = allAssets.GetEnumerator();
			for (var i = 0; i < allAssets.Length; i++)
			{
				assets.Add(allAssets[i], bundle);
			}
		}
        private static string[] LoadAllBundleName(AssetBundle manifestBundle)
		{
			var manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			string[] bundles = manifest.GetAllAssetBundles();
			List<string> updateList = new List<string>();
			for (var i = 0; i < bundles.Length; i++)
			{
				var file = bundles[i];
				var ext = Path.GetExtension(file);
				var name = Path.GetFileNameWithoutExtension(file);
				var tmp = name.Split('_');
				string shortName;
				if (tmp.Length > 1) 
				{
					shortName = tmp[0];
				}
				else               
				{
					shortName = name;
				}
                // 变体为一个新的bundle
                if (ext != "")
				{
					shortName += ext;
				}
				if (hashBundleMap.ContainsKey(shortName))
				{
					if (file == hashBundleMap[shortName])
					{                  
                        Debug.Log("Nochange map:" + shortName + "=>" + file);
					}
					else
					{
						updateList.Add(shortName);
						hashBundleMap[shortName] = file;
                        Debug.Log("Update map:" + shortName + "=>" + file);
					}               
				}
				else
				{
					hashBundleMap.Add(shortName, file);
                    Debug.Log("Add map:" + shortName + "=>" + file);
				}

			}
			return updateList.ToArray();
		}
        // hash的bundle映射
		private static string GetHashBundleName(string bundleName, string variant)
		{
			string name;
			if (variant != null) bundleName = bundleName + "." + variant;
			var result = hashBundleMap.TryGetValue(bundleName, out name);
            if (result)
			{
				Debug.Log("Hit bundle:" + bundleName + "->"+name);
				return name;
			}
			else
			{
				Debug.Log("Miss bundle:" + bundleName);
				return bundleName;
			}
		}

        // Assets会直接打包，不应该存在这类模式
		//private static IEnumerator LoadDataBundle(string bundleName)
		//{
		//	var levelList = bundleMap[bundleLevel];

		//	bundleName = GetHashBundleName(bundleName);
		//	if (loadBundleList.Contains(bundleName))
  //          {
  //              yield break;
  //          }

		//	var bundle = AssetBundle.LoadFromFile("file://" + Application.dataPath + "/" + bundleName);
            
		//	var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		//	string[] dependencies = manifest.GetAllDependencies("assetBundle"); //Pass the name of the bundle you want the dependencies for.
		//	for (var i = 0; i < dependencies.Length; i++)
  //          {
  //              var dependency = dependencies[i];
  //              dependency = GetHashBundleName(dependency);
		//		var dependencyBundle = AssetBundle.LoadFromFile("file://" + Application.dataPath + "/AssetBundles/" + Build + "/" + dependency);
		//		levelList.Add(dependencyBundle);
		//		LoadAllAssetsName(dependencyBundle);
		//	}
		//	levelList.Add(bundle);
		//	LoadAllAssetsName(bundle);
		//	yield return null;
		//}
        
		private static IEnumerator InitBundle(bool stream=true)
        {
			// 读取清单文件
			if (!AssetBundleStatus)
			{
				yield break;
			}
			yield break;
            string uri;
			if (CanUpdateBundle)
			{               
                uri = GetBundleUrl(Build, null);
			}
			else
			{
				uri = GetBundleStream(Build, null);
			}
			Debug.Log("Init Bundle Mainfest:" + uri);
			UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, Hash, 0);
            yield return request.Send();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
			LoadAllBundleName(bundle);
			manifestBundle = bundle;
            
			//var commonLevel = new List<string>();
			//levelMap[bundleLevel] = commonLevel;
			//if (Settings.CommonBundleName != null && Settings.CommonBundleName != "")
			//{
				//Debug.Log("Load Common Bundle:" + Settings.CommonBundleName);
				//if (stream)
				//{
				//	yield return LoadStreamBundle(Settings.CommonBundleName, null, null);
				//}
				//else
				//{
				//	yield return LoadBundleInline(Settings.CommonBundleName, null, null);
				//}
            //}
        }
        
		public static object LoadAsset<T>(string name)
		{
			if (!AssetBundleStatus) {
				return Resources.Load(name);
			}
			AssetBundle bundle;
			var e = assetMap.Values.GetEnumerator();
			while(e.MoveNext())
			{            
				var result = e.Current.TryGetValue(name, out bundle);
				if (!result){
					continue;
				}
				return bundle.LoadAsset(name);
			}
			Debug.LogError(string.Format("Asset:{0} not in Bundles", name));
			return null;
		}

		private static string GetBundleUrl(string bundleName, string variant)
		{
			bundleName = GetHashBundleName(bundleName, variant);
			var path = Settings.BundlePath.Replace("{version}", localInfo.version.ToString()).Replace("{build}", Build).Replace("{time}", localInfo.resourceVersion);
			string uri = Path.Combine(path, bundleName);
			Debug.Log("Web Bundle :" + bundleName + "->" + uri);
			return uri;
		}

		private static string GetBundleStream(string bundleName, string variant)
		{         
            bundleName = GetHashBundleName(bundleName, variant);
#if UNITY_ANDROID
            string uri = Application.streamingAssetsPath + "/" + bundleName;
#else
            string uri = "file://" + Application.streamingAssetsPath + "/" + bundleName;
#endif
			Debug.Log("Stream Bundle :" + bundleName + "->" + uri);
			return uri;
		}

		private static IEnumerator LoadBundleInline(string bundleName, string variant, Action<int> callback)
		{
			Debug.Log("Load Web Bundle:" + bundleName);
			if (loadBundleMap.ContainsKey(bundleName))
            {
                if (callback != null)
                    callback(0);
                yield break;
            }
			string uri = GetBundleUrl(bundleName, variant);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, Hash, 0);
            yield return request.Send();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle == null)
			{
				if (callback != null)
                    callback(0);
				yield break;
			}
            var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] dependencies = manifest.GetAllDependencies("assetBundle"); //Pass the name of the bundle you want the dependencies for.
            for (var i = 0; i < dependencies.Length; i++)
            {
                var dependency = dependencies[i];
				yield return LoadBundleInline(dependency, null, null);
				if (callback != null)
					callback(dependencies.Length - i);
            }
			LoadAllAssetsName(bundleName, bundle);
            if (callback != null)
                callback(0);
		}

		private static IEnumerator LoadStreamBundle(string bundleName, string variant, Action<int> callback)
        {
			Debug.Log("Load Stream BUndle:" + bundleName);
			if (loadBundleMap.ContainsKey(bundleName))
			{
                if (callback != null)
                    callback(0);
                yield break;
            }
			var uri = GetBundleStream(bundleName, variant);
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, Hash, 0);
            yield return request.Send();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
			if (bundle == null)
            {
                if (callback != null)
                    callback(0);
                yield break;
            }
            var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] dependencies = manifest.GetAllDependencies("assetBundle"); //Pass the name of the bundle you want the dependencies for.
            for (var i = 0; i < dependencies.Length; i++)
            {
                var dependency = dependencies[i];
                if (CanUpdateApp)
                {
					yield return LoadBundleInline(dependency, null, null);
                }
                else
                {
					yield return LoadStreamBundle(dependency, null, null);
				}
                if (callback != null)
                    callback(dependencies.Length - i);
            }
			LoadAllAssetsName(bundleName, bundle);
            if (callback != null)
                callback(0);
        }
        
		public static IEnumerator LoadBundle(string assetBundleName, string variant, Action<int> callback)
		{
			if (!AssetBundleStatus)
            {
				callback(0);
            }
			else if (CanUpdateApp)
			{
				yield return LoadBundleInline(assetBundleName, variant, callback);
			}
			else
			{            
				yield return LoadStreamBundle(assetBundleName, variant, callback);
			}
        }

		public static void PushBundle()
		{
			if (!AssetBundleStatus)
            {
				return;
            }
			bundleLevel++;         
            var bundleList = new List<string>();
			levelMap[bundleLevel] = bundleList;
		}

        public static void UnloadBundle()
		{
			if (!AssetBundleStatus)
            {
                return;
            }
			var list = levelMap[bundleLevel];
            var e = list.GetEnumerator();
            while (e.MoveNext())
            {
				var bundle = loadBundleMap[e.Current];
				if (bundle != null)
					bundle.Unload(false);
            }
		}

        public static void PopBundle()
		{
			if (!AssetBundleStatus)
            {
                return;
            }
			assetMap.Remove(bundleLevel);
			var list = levelMap[bundleLevel];
			var e = list.GetEnumerator();
			while (e.MoveNext())
			{
				var bundle = loadBundleMap[e.Current];
                if (bundle != null)
				{
					bundle.Unload(true);
					loadBundleMap.Remove(e.Current);
				}
			}
			levelMap.Remove(bundleLevel);
			bundleLevel--;
		}


        public static void parse(AssetBundle bundle)
        {
            TextAsset txt = bundle.LoadAsset("myBinaryAsText", typeof(TextAsset)) as TextAsset;

            // Load the assembly and get a type (class) from it
            var assembly = System.Reflection.Assembly.Load(txt.bytes);
            var type = assembly.GetType("MyClassDerivedFromMonoBehaviour");

        }
    }
}