using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
	public class Game:MonoBehaviour
	{
		public static Game single;

		private bool isMusicPlaying = false;
		private AudioSource audioMusicSource;
		private bool musicMute = false;
		private AudioSource audioSoundSource;
		private bool soundMute = false;

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

		public static int getLanguage ()
		{
			string lg = Application.systemLanguage.ToString ();
			if (lg == "ChineseSimplified" || lg == "ChineseTraditional" || lg == "Chinese") {
				return LG.ZH;
			} else {
				return LG.EN;
			}
		}

		public static string getVersion() {
			return Application.unityVersion;
		}

		public static string getDevice () {
			return Application.platform.ToString();
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

		public void waitSeconds (float time, Action handle)
		{
			StartCoroutine (waitSecondsAsync (time, handle));
		}

		IEnumerator waitSecondsAsync (float time, Action handle)
		{
			yield return new WaitForSeconds (time);
			handle ();
		}

		public void loadImage (string url, Action<Texture2D> callback)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (loadUrlAsyn (url, "imageTmp", "png", (www) => {callback(www.texture);}));
		}

		public void loadAssetbundle (string url, Action<AssetBundle> callback)
		{
			if (url == null || url == "")
				return;
			StartCoroutine (loadUrlAsyn (url, "assetBundleTmp", "model", (www) => {callback(www.assetBundle.mainAsset);}));
		}


		IEnumerator loadUrlAsyn (string url, string folderName, string suffix, Action<WWW> callback)
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
				callback (www);
				if (!local) {
					FileTools.createFolder (path, folderName);
					FileTools.saveFile (path + "/" + folderName + "/" + fileName, info);
				}
			}
		}
	}
}