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

		public IEnumerator Get (string url, Dictionary<string,string> data, Dictionary<string,string> headers, Action<string,WWW> callBack = null)
		{
			Dictionary<string, string>.Enumerator etor = data.GetEnumerator ();
			url += "?";
			while (etor.MoveNext ()) {
				url += etor.Current.Key + "=" + WWW.EscapeURL (etor.Current.Value) + "&";
			}
			url = url.Remove (-1);
			return IESend (url, null, headers, callBack);
		}

		public IEnumerator Post (string url, Dictionary<string,string> data, Dictionary<string,string> headers, Action<string,WWW> callBack = null)
		{
			WWWForm wf = new WWWForm ();
			Dictionary<string, string>.Enumerator etor = data.GetEnumerator ();
			while (etor.MoveNext ()) {
				wf.AddField (etor.Current.Key, etor.Current.Value);
			}
			return Post (url, wf, headers, callBack);
		}

		public IEnumerator Post (string url, string data, Dictionary<string, string> headers, Action<string,WWW> callBack = null)
		{
			byte[] postData = System.Text.UTF8Encoding.UTF8.GetBytes (data); 
			return Post (url, postData, headers, callBack);
		}

		public IEnumerator Post (string url, byte[] data, Dictionary<string, string> headers, Action<string, WWW> callBack = null)
		{
			return IESend (url, data, headers, callBack);
		}

		public IEnumerator Post (string url, WWWForm form, Dictionary<string, string> headers, Action<string, WWW> callBack = null)
		{
			Dictionary<string, string>.Enumerator etorHeader = form.headers.GetEnumerator ();
			while (etorHeader.MoveNext ()) {
				headers.Add (etorHeader.Current.Key, etorHeader.Current.Value);
			}
			return IESend (url, form.data, headers, callBack);
		}

		public IEnumerator WrapGet (string url, Dictionary<string,string> data, Action<JSONNode> success = null, Action<string> fail = null)
		{
			Dictionary<string,string> headers = new Dictionary<string, string> ();
			headers.Add ("Content-type", "application/json");
			return Get (url, data, headers, (result, www) => {
				if (result == null) {
					if (fail != null)
						fail (www.error);
					return;
				}
				var o = JSONNode.Parse (result);
				if (o ["status"].AsInt != 200) {
					if (fail != null)
						fail (o ["message"]);
				} else {
					if (success != null)
						success (o ["result"].AsObject);
				}
			});

		}

		public IEnumerator WrapPost (string url, Dictionary<string,string> data, Action<JSONNode> success = null, Action<string> fail = null)
		{
			Dictionary<string,string> headers = new Dictionary<string, string> ();
			headers.Add ("Content-type", "application/json");
			return Post (url, data, headers, (result, www) => {
				if (result == null) {
					if (fail != null)
						fail (www.error);
					return;
				}
				var o = JSONNode.Parse (result);
				if (o ["status"].AsInt != 200) {
					if (fail != null)
						fail (o ["message"]);
				} else {
					if (success != null)
						success (o ["result"].AsObject);
				}
			});
		}

		public IEnumerator WrapPostForm (string url, WWWForm form, Action<JSONNode> success = null, Action<string> fail = null)
		{
			Dictionary<string, string> headers = new Dictionary<string, string> ();
			return Post (url, form, headers, (result, www) => {
				if (result == null) {
					if (fail != null)
						fail (www.error);
					return;
				}
				var o = JSONNode.Parse (result);
				if (o ["status"].AsInt != 200) {
					if (fail != null)
						fail (o ["message"]);
				} else {
					if (success != null)
						success (o ["result"].AsObject);
				}
			});
		}

		IEnumerator IESend (string url, byte[] data, Dictionary<string,string> headers, Action<string,WWW> callBack)
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

			if (callBack != null) {
				if (www.error != null) {
					callBack (null, www);
				} else {
					callBack (www.text, www);
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

		public void uploadOSS(string filepath, JSONNode result, string mime, Action<string> callback){
			string host = result["host"];
			string key = result["key"];
			string OSSAccessKeyId = result["OSSAccessKeyId"];
			string policy = result["policy"];
			string signature = result["signature"];
			string url = result["url"];
			FileInfo file = new FileInfo(filepath);
			WWWForm wf = new WWWForm();
			Dictionary<string, string> headers = new Dictionary<string, string>();
			var boundary = "--" + Tools.generateStr(32) + "--";
			headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
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
			var requestBodyByte = System.Text.Encoding.Default.GetBytes(requestBody);
			var fileByte = File.ReadAllBytes(filepath);

			var lastBody = "\r\n--" + boundary + "--\r\n";
			var requestlastBodyByte = System.Text.Encoding.Default.GetBytes(lastBody);

			//headers.Add("Content-Length", requestBody.Length.ToString());
			//wf.AddField("OSSAccessKeyId", OSSAccessKeyId);
			//wf.AddField("policy", policy);
			//wf.AddField("Signature", signature);
			//wf.AddField("key", key);
			//wf.AddField("success_action_status", result["success_action_status"].AsInt);
			//wf.AddBinaryData("file", File.ReadAllBytes(filepath), file.Name, mime);
			var by = new byte[requestBodyByte.Length + fileByte.Length + requestlastBodyByte.Length];

			Array.Copy(requestBodyByte, 0, by, 0, requestBodyByte.Length);
			Array.Copy(fileByte, 0, by, requestBodyByte.Length, fileByte.Length);
			Array.Copy(requestlastBodyByte, 0, by, requestBodyByte.Length + fileByte.Length, requestlastBodyByte.Length);
			StartCoroutine(Post(result["host"], by, headers, (body, www) => {
				if(body == null)
				{
					callback(null);
				}
				else
				{
					callback(url.Replace("{key}", key));
				}
			}));
		}
	}
}

