﻿using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Gj.Galaxy.Logic;

namespace Gj
{
	public class Game:MonoBehaviour
	{
		public static Game single;

		private AudioSource audioMusicSource;
		private AudioSource audioSoundSource;

		private List<string> sceneList;

		static Game ()
		{
			GameObject game = new GameObject ("Game");
			DontDestroyOnLoad (game);
			single = game.AddComponent<Game> ();
			single.audioMusicSource = game.AddComponent<AudioSource> ();
			single.audioMusicSource.volume = Cache.musicVolume;
			single.audioSoundSource = game.AddComponent<AudioSource> ();
			single.audioSoundSource.volume = Cache.soundVolume;

            ServerSettings settings = PeerClient.Generate();
            settings.SetAppInfo("first", "1.0.1", "xasfd");
            settings.HostType = ServerSettings.HostingOption.OnlineMode;
			settings.ServerAddress = "192.168.31.225:8080";

            PeerClient.Listener.OnReconnectEvent += (bool success) =>
            {
                if (success)
                {
                }
                else
                {
                    Debug.LogError("Server reconnect fail");
                }

            };
            PeerClient.Connect();
		}

		public static string GetVersion ()
		{
			return Application.unityVersion;
		}

		public static string GetDevice ()
		{
			return Application.platform.ToString ();
		}

        public void InitScene (string sceneName) {
            if (sceneList == null)
            {
                sceneList = new List<string>();
            }
            sceneList.Add(sceneName);
        }

		public void JumpScene (string sceneName)
		{
			if (sceneList == null) {
				sceneList = new List<string> ();
			}
			sceneList.Add (sceneName);
            Application.LoadLevel(sceneName);
		}

		public void BackScene ()
		{
			string sceneName = "main";
			if (sceneList != null && sceneList.Count > 0) {
				sceneList.RemoveAt (sceneList.Count - 1);
			}
			if (sceneList != null && sceneList.Count > 0) {
				sceneName = sceneList [sceneList.Count - 1];
            }
            Application.LoadLevel(sceneName);
		}

		public void ChangeSpeed (float value)
		{
			Time.timeScale = 1.0f * value;
		}

		public void RecoverySpeed ()
		{
			Time.timeScale = 1.0f;
		}

		public void ChangeMusicVolume (float value)
		{
			Cache.musicVolume = value;
			audioMusicSource.volume = Cache.musicVolume;
		}

		public void ChangeSoundVolume (float value)
		{
			Cache.soundVolume = value;
			audioSoundSource.volume = Cache.soundVolume;
		}

		public void PlayMusic (string tag)
		{
			AudioClip audioClip = Resource.GetAudioClip (tag);
			if (audioClip != null) {
				audioMusicSource.PlayOneShot (audioClip);
			}
		}

		public void PlaySound (string tag)
		{
			AudioClip audioClip = Resource.GetAudioClip (tag);
			if (audioClip != null) {
				audioSoundSource.PlayOneShot (audioClip);
			}
		}

		public void PlaySound (string tag, Vector3 position)
		{
			AudioClip audioClip = Resource.GetAudioClip (tag);
			if (audioClip != null) {
				AudioSource.PlayClipAtPoint (audioClip, position, Cache.soundVolume);
			}
		}

		public void WaitSeconds (float time, Action CB)
		{
			StartCoroutine (WaitSecondsAsync (time, CB));
		}

		IEnumerator WaitSecondsAsync (float time, Action CB)
		{
			yield return new WaitForSeconds (time);
			CB ();
		}

		public void LoadImage (string url, Action<bool, Texture2D, string> CB)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (LoadUrlAsyn (url, "imageTmp", "png", (success, www, message) => {
				if (success) {
					CB (true, www.texture, "success");
				} else {
					CB (false, null, message);
				}
			}));
		}

		public void LoadAssetbundle (string url, Action<bool, AssetBundle, string> CB)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (LoadUrlAsyn (url, "assetBundleTmp", "model", (success, www, message) => {
				if (success) {
					CB (true, www.assetBundle, "success");
				} else {
					CB (false, null, message);
				}
			}));
		}


		IEnumerator LoadUrlAsyn (string url, string folderName, string suffix, Action<bool, WWW, string> CB)
		{
			string path = Application.persistentDataPath;
			string fileName = SimpleTools.Md5 (url) + "." + suffix;
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
					FileTools.CreateFolder (path, folderName);
					FileTools.SaveFile (path + "/" + folderName + "/" + fileName, www.bytes);
				}
			} else {
				CB (false, null, "is error!");
			}
		}

		public void UploadOSS (string filepath, JSONNode result, string mime, Action<bool, string, string> CB)
		{
			string host = result ["host"];
			string key = result ["key"];
			string OSSAccessKeyId = result ["OSSAccessKeyId"];
			string policy = result ["policy"];
			string signature = result ["signature"];
			string url = result ["url"];
			FileInfo file = new FileInfo (filepath);
			Dictionary<string, string> headers = new Dictionary<string, string> ();
			var boundary = "--" + SimpleTools.GenerateStr (32) + "--";
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
			StartCoroutine (Http.GetInstance ().Post (result ["host"], by, headers, (success, www, message) => {
				if (success) {
					CB (true, url.Replace ("{key}", key), "success");
				} else {
					CB (false, null, message);
				}

			}));
		}
	}
}