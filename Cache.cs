using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

namespace Gj
{
	public class Cache
	{
		public string identity {
			get {
				return PlayerPrefs.GetString ("userIdentity", "identity");
			}
			set {
				PlayerPrefs.SetString ("userIdentity", value);
			}
		}

		public string name {
			get {
				return PlayerPrefs.GetString ("userName", "");
			}
			set {
				PlayerPrefs.SetString ("userName", value);
			}
		}

		public string platform {
			get {
				return PlayerPrefs.GetString ("userPlatform", "");
			}
			set {
				PlayerPrefs.SetString ("userPlatform", value);
			}
		}

		public string token {
			get {
				return PlayerPrefs.GetString ("userToken", "");
			}
			set {
				PlayerPrefs.SetString ("userToken", value);
			}
		}

		public int id {
			get {
				return PlayerPrefs.GetInt ("userId", 0);
			}
			set {
				PlayerPrefs.SetInt ("userId", value);
			}
		}

		public static float musicVolume {
			get {
				return PlayerPrefs.GetFloat ("envMusicVolume", 1);
			}
			set {
				PlayerPrefs.SetFloat ("envMusicVolume", value);
			}
		}

		public static float soundVolume {
			get {
				return PlayerPrefs.GetFloat ("soundVolume", 1);
			}
			set {
				PlayerPrefs.SetFloat ("soundVolume", value);
			}
		}

		public static float speed {
			get {
				return PlayerPrefs.GetFloat ("gameSpeed", 1);
			}
			set {
				PlayerPrefs.SetFloat ("gameSpeed", value);
			}
		}

		public static string getVal (string key, string defalut) {
			return PlayerPrefs.GetString (key, defalut);
		}

		public static void setVal(string key, string value) {
			PlayerPrefs.SetString (key, value);
		}

		public static int getVal (string key, int defalut) {
			return PlayerPrefs.GetInt (key, defalut);
		}

		public static void setVal(string key, int value) {
			PlayerPrefs.SetInt (key, value);
		}

		public static float getVal (string key, float defalut) {
			return PlayerPrefs.GetFloat (key, defalut);
		}

		public static void setVal(string key, float value) {
			PlayerPrefs.SetFloat (key, value);
		}

		public static bool getVal (string key, bool defalut) {
			return PlayerPrefs.GetBool (key, defalut);
		}

		public static void setVal(string key, bool value) {
			PlayerPrefs.SetBool (key, value);
		}

		public static long getVal (string key, long defalut) {
			return PlayerPrefs.GetLong (key, defalut);
		}

		public static void setVal(string key, long value) {
			PlayerPrefs.SetLong (key, value);
		}
	}
}