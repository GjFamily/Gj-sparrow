using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace Gj
{
	public class Game:MonoBehaviour
	{
		public static Game single;

		private AudioSource audioMusicSource;
		private AudioSource audioSoundSource;

		private List<string> sceneList;

		public enum LG
		{
			ZH,
			EN
		}

		static Game ()
		{
			GameObject game = new GameObject ("Game");
			DontDestroyOnLoad (game);
			single = game.AddComponent<Game> ();
			single.audioMusicSource = game.AddComponent<AudioSource> ();
			single.audioMusicSource.volume = Cache.musicVolume;
			single.audioSoundSource = game.AddComponent<AudioSource> ();
			single.audioSoundSource.volume = Cache.soundVolume;
		}

		public static LG getLanguage ()
		{
			string lg = Application.systemLanguage.ToString ();
			if (lg == "ChineseSimplified" || lg == "ChineseTraditional" || lg == "Chinese") {
				return LG.ZH;
			} else {
				return LG.EN;
			}
		}

		public static string getVersion ()
		{
			return Application.unityVersion;
		}

		public static string getDevice ()
		{
			return Application.platform.ToString ();
		}

		public void addScene (string sceneName)
		{
			if (sceneList == null) {
				sceneList = new List<string> ();
			}
			sceneList.Add (sceneName);
		}

		public string getLastScene ()
		{
			string sceneName = "main";
			if (sceneList != null && sceneList.Count > 0) {
				sceneList.RemoveAt (sceneList.Count - 1);
			}
			if (sceneList != null && sceneList.Count > 0) {
				sceneName = sceneList [sceneList.Count - 1];
			}
			return sceneName;
		}

		public void changeSpeed (float value)
		{
			Time.timeScale = 1.0f * value;
		}

		public void recoverySpeed ()
		{
			Time.timeScale = 1.0f;
		}

		public void changeMusicVolume (float value)
		{
			Cache.musicVolume = value;
			audioMusicSource.volume = Cache.musicVolume;
		}

		public void changeSoundVolume (float value)
		{
			Cache.soundVolume = value;
			audioSoundSource.volume = Cache.soundVolume;
		}

		public void playMusic (string tag)
		{
			AudioClip audioClip = Resource.getAudioClip (tag);
			if (audioClip != null) {
				audioMusicSource.PlayOneShot (audioClip);
			}
		}

		public void playSound (string tag)
		{
			AudioClip audioClip = Resource.getAudioClip (tag);
			if (audioClip != null) {
				audioSoundSource.PlayOneShot (audioClip);
			}
		}

		public void playSound (string tag, Vector3 position)
		{
			AudioClip audioClip = Resource.getAudioClip (tag);
			if (audioClip != null) {
				AudioSource.PlayClipAtPoint (audioClip, position, Cache.soundVolume);
			}
		}

		public void waitSeconds (float time, Action CB)
		{
			StartCoroutine (waitSecondsAsync (time, CB));
		}

		IEnumerator waitSecondsAsync (float time, Action CB)
		{
			yield return new WaitForSeconds (time);
			CB ();
		}

		public void loadImage (string url, Action<bool, Texture2D, string> CB)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (loadUrlAsyn (url, "imageTmp", "png", (success, www, message) => {
				if (success) {
					CB (true, www.texture, "success");
				} else {
					CB (false, null, message);
				}
			}));
		}

		public void loadAssetbundle (string url, Action<bool, AssetBundle, string> CB)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (loadUrlAsyn (url, "assetBundleTmp", "model", (success, www, message) => {
				if (success) {
					CB (true, www.assetBundle, "success");
				} else {
					CB (false, null, message);
				}
			}));
		}


		IEnumerator loadUrlAsyn (string url, string folderName, string suffix, Action<bool, WWW, string> CB)
		{
			string path = Application.persistentDataPath;
			string fileName = Tools.md5 (url) + "." + suffix;
			string localUrl = path + "/" + folderName + "/" + fileName;
			bool local = false;
			WWW www;
			if (new FileInfo (localUrl).Exists) {
				www = new WWW ("file://" + localUrl);
				local = true;
			} else {
				www = new WWW (url);
			}
			yield return www;
			if (www != null && www.isDone && string.IsNullOrEmpty (www.error)) {
				CB (true, www, "success");
				if (!local) {
					FileTools.createFolder (path, folderName);
					FileTools.saveFile (path + "/" + folderName + "/" + fileName, www.bytes);
				}
			} else {
				CB (false, null, "is error!");
			}
		}

		public void uploadOSS (string filepath, JSONNode result, string mime, Action<bool, string, string> CB)
		{
			string host = result ["host"];
			string key = result ["key"];
			string OSSAccessKeyId = result ["OSSAccessKeyId"];
			string policy = result ["policy"];
			string signature = result ["signature"];
			string url = result ["url"];
			FileInfo file = new FileInfo (filepath);
			Dictionary<string, string> headers = new Dictionary<string, string> ();
			var boundary = "--" + Tools.generateStr (32) + "--";
			headers.Add ("Content-Type", "multipart/form-data; boundary=" + boundary);
			var requestBody = "--" + boundary + "\r\n"
			                  + "Content-Disposition: form-data; name=\"key\"\r\n"
			                  + "\r\n" + key + "\r\n"
			                  + "--" + boundary + "\r\n"
			                  + "Content-Disposition: form-data; name=\"OSSAccessKeyId\"\r\n"
			                  + "\r\n" + OSSAccessKeyId + "\r\n"
			                  + "--" + boundary + "\r\n"
			                  + "Content-Disposition: form-data; name=\"policy\"\r\n"
			                  + "\r\n" + policy + "\r\n"
			                  + "--" + boundary + "\r\n"
			                  + "Content-Disposition: form-data; name=\"Signature\"\r\n"
			                  + "\r\n" + signature + "\r\n"
			                  + "--" + boundary + "\r\n"
			                  + "Content-Disposition: form-data; name=\"file\"; filename=\"" + file.Name + "\"\r\n"
			                  + "Content-Type: " + mime + "\r\n"
			                  + "\r\n";
			var requestBodyByte = System.Text.Encoding.Default.GetBytes (requestBody);
			var fileByte = File.ReadAllBytes (filepath);

			var lastBody = "\r\n--" + boundary + "--\r\n";
			var requestlastBodyByte = System.Text.Encoding.Default.GetBytes (lastBody);

			//WWWForm wf = new WWWForm();
			//headers.Add("Content-Length", requestBody.Length.ToString());
			//wf.AddField("OSSAccessKeyId", OSSAccessKeyId);
			//wf.AddField("policy", policy);
			//wf.AddField("Signature", signature);
			//wf.AddField("key", key);
			//wf.AddField("success_action_status", result["success_action_status"].AsInt);
			//wf.AddBinaryData("file", File.ReadAllBytes(filepath), file.Name, mime);
			var by = new byte[requestBodyByte.Length + fileByte.Length + requestlastBodyByte.Length];

			Array.Copy (requestBodyByte, 0, by, 0, requestBodyByte.Length);
			Array.Copy (fileByte, 0, by, requestBodyByte.Length, fileByte.Length);
			Array.Copy (requestlastBodyByte, 0, by, requestBodyByte.Length + fileByte.Length, requestlastBodyByte.Length);
			StartCoroutine (Http.getInstance ().Post (result ["host"], by, headers, (success, www, message) => {
				if (success) {
					CB (true, url.Replace ("{key}", key), "success");
				} else {
					CB (false, null, message);
				}

			}));
		}
	}
}