using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Net;
using System.IO;
using SimpleJSON;

namespace Gj
{
	public class Http
	{
		private static Http instance = null;
		private static object _lock = new object ();
		private static CookieJar cookieJar = new CookieJar ();

		private Http ()
		{
		}

		public static Http getInstance ()
		{
			if (instance == null) {
				lock (_lock) {
					if (instance == null) {
						instance = new Http ();
					}
				}
			}
			return instance;
		}

		public IEnumerator Get (string url, Dictionary<string,string> data, Dictionary<string,string> headers, Action<bool,WWW,string> CB)
		{
			Dictionary<string, string>.Enumerator etor = data.GetEnumerator ();
			url += "?";
			while (etor.MoveNext ()) {
				url += etor.Current.Key + "=" + WWW.EscapeURL (etor.Current.Value) + "&";
			}
			url = url.Remove (-1);
			return IESend (url, null, headers, CB);
		}

		public IEnumerator Post (string url, Dictionary<string,string> data, Dictionary<string,string> headers, Action<bool,WWW,string> CB)
		{
			WWWForm wf = new WWWForm ();
			Dictionary<string, string>.Enumerator etor = data.GetEnumerator ();
			while (etor.MoveNext ()) {
				wf.AddField (etor.Current.Key, etor.Current.Value);
			}
			return Post (url, wf, headers, CB);
		}

		public IEnumerator Post (string url, string data, Dictionary<string, string> headers, Action<bool,WWW,string> CB)
		{
			byte[] postData = System.Text.UTF8Encoding.UTF8.GetBytes (data); 
			return Post (url, postData, headers, CB);
		}

		public IEnumerator Post (string url, byte[] data, Dictionary<string, string> headers, Action<bool,WWW,string> CB)
		{
			return IESend (url, data, headers, CB);
		}

		public IEnumerator Post (string url, WWWForm form, Dictionary<string, string> headers, Action<bool,WWW,string> CB)
		{
			Dictionary<string, string>.Enumerator etorHeader = form.headers.GetEnumerator ();
			while (etorHeader.MoveNext ()) {
				headers.Add (etorHeader.Current.Key, etorHeader.Current.Value);
			}
			return IESend (url, form.data, headers, CB);
		}

		public IEnumerator WrapGet (string url, Dictionary<string,string> data, Action<bool, JSONNode, string> CB)
		{
			Dictionary<string,string> headers = new Dictionary<string, string> ();
			headers.Add ("Content-type", "application/json");
			return Get (url, data, headers, (success, www, message) => {
				if (!success) {
					CB (false, null, message);
				}
				var o = JSONNode.Parse (www.text);
				if (o ["status"].AsInt != 200) {
					CB(false, null, o["message"]);
				} else {
					CB(true, o ["result"].AsObject, "success");
				}
			});

		}

		public IEnumerator WrapPost (string url, Dictionary<string,string> data, Action<bool, JSONNode, string> CB)
		{
			Dictionary<string,string> headers = new Dictionary<string, string> ();
			headers.Add ("Content-type", "application/json");
			return Post (url, data, headers, (success, www, message) => {
				if (!success) {
					CB (false, null, message);
				}
				var o = JSONNode.Parse (www.text);
				if (o ["status"].AsInt != 200) {
					CB(false, null, o["message"]);
				} else {
					CB(true, o ["result"].AsObject, "success");
				}
			});
		}

		public IEnumerator WrapPostForm (string url, WWWForm form, Action<bool, JSONNode, string> CB)
		{
			Dictionary<string, string> headers = new Dictionary<string, string> ();
			return Post (url, form, headers, (success, www, message) => {
				if (!success) {
					CB (false, null, message);
				}
				var o = JSONNode.Parse (www.text);
				if (o ["status"].AsInt != 200) {
					CB(false, null, o["message"]);
				} else {
					CB(true, o ["result"].AsObject, "success");
				}
			});
		}

		IEnumerator IESend (string url, byte[] data, Dictionary<string,string> headers, Action<bool,WWW,string> CB)
		{
			headers.Add ("Cookie", cookieJar.generate (url));
			WWW www = new WWW (url, data, headers);
			yield return www;
			if (www.error != null) {
				Debug.LogError (www.error);
				Debug.LogError (www.text);
			} else {
				cookieJar.parse (www);
			}

			if (CB != null) {
				if (www.error != null) {
					CB (false, www, www.error);
				} else {
					CB (true, www, "success");
				}
			}

		}

		public class CookieJar
		{
			private CookieContainer cc = new CookieContainer ();

			public void set (string name, string value)
			{
				Cookie c = new Cookie (name, value);
				cc.Add (c);
			}

			public void set (string name, string value, string path, string domain)
			{
				Cookie c = new Cookie (name, value, path, domain);
				cc.Add (c);
			}

			public void parse (WWW www)
			{
				if (!www.responseHeaders.ContainsKey ("SET-COOKIE")) {
					return;
				}

				var rhsPropInfo = typeof(WWW).GetProperty ("responseHeadersString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (rhsPropInfo == null) {
					Debug.LogError ("www.responseHeadersString not found in WWW class.");
					return;
				}
				var headersString = rhsPropInfo.GetValue (www, null) as string;
				if (headersString == null) {
					return;
				}
				Uri uri = new Uri (www.url);
				string[] lines = headersString.Split (new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var l in lines) {
					var colIdx = l.IndexOf (':');
					if (colIdx < 1) {
						continue;
					}
					var headerType = l.Substring (0, colIdx).Trim ();
					if (headerType.ToUpperInvariant () != "SET-COOKIE") {
						continue;
					}
					var headerVal = l.Substring (colIdx + 1).Trim ();
					string[] items = headerVal.Split (new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
					string[] info = items [0].Split (new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					if (info.Length != 2) {
						continue;
					}
					set (info [0], info [1], "/", uri.Host);
				}
			}

			public string generate (string url)
			{
				string cookie = cc.GetCookieHeader (new Uri (url));

				return cookie;
			}
		}
	}
}

