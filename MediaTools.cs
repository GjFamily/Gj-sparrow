using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using AOT;

namespace Gj
{
	class MediaTools
	{
		static private bool __recording = false;
		static private bool __inter_recording = false;
		static private bool __generating = false;
		static private string mediaAllPath = "";
		static private Action errorCallBack;
		static private Action<bool> finishCallBack;
		static private Action generateCallBack;

		static private int __x = 0;
		static private int __y = 0;
		static private int __width = 0;
		static private int __height = 0;

		//	#if !UNITY_EDITOR
		static private IntPtr _session;
		//	#endif
		public static IEnumerator photo (Camera camera, int x, int y, int width, int height, Action<bool, byte[], string> CB)
		{
			yield return new WaitForEndOfFrame ();
			//		yield return new WaitForFixedUpdate ();
			RenderTexture rt = new RenderTexture (width, height, 24);
			Texture2D photo = new Texture2D (width, height, TextureFormat.RGB24, false);
			RenderTexture tmp = RenderTexture.active;
			camera.targetTexture = rt;
			camera.Render ();
			RenderTexture.active = rt;
//
			photo.ReadPixels (new Rect (x, y, width, height), 0, 0, false);
			photo.Apply ();
			camera.targetTexture = null;
			RenderTexture.active = tmp;
			byte[] bs = photo.EncodeToPNG ();
			CB (true, bs, "message");
		}

		public static void GenerateMovieImage (string movie, Action<bool, string, string> CB)
		{
			if (__generating) {
				Debug.Log ("Generating, do not start again");
				CB (false, null, "Generating, do not start again");
				return;
			}
			var file = Application.persistentDataPath + "/" + Tools.generateStr (10) + ".png";
			generateCallBack = () => {
				CB (true, file, "success");
			};
			unity_movieToImage (movie, file, _movie_imageCallBack);
		}

		[MonoPInvokeCallback (typeof(internal_movie_imageCallBack))]
		public static void _movie_imageCallBack ()
		{
			__generating = false;
			if (generateCallBack != null)
				generateCallBack ();
		}

		[DllImport ("__Internal")]
		private static extern IntPtr unity_createRecordSession ();

		delegate void internal_audioEncodeErrorCallBack ();

		[DllImport ("__Internal")]
		private static extern bool unity_preAudio (IntPtr session, internal_audioEncodeErrorCallBack encodeErrorCallBack);

		delegate void internal_videoEncodeErrorCallBack ();

		[DllImport ("__Internal")]
		private static extern bool unity_preVideo (IntPtr session, int width, int height, int fps, internal_videoEncodeErrorCallBack encodeErrorCallBack);

		[DllImport ("__Internal")]
		private static extern bool unity_startRecord (IntPtr session, string file);

		[DllImport ("__Internal")]
		private static extern bool unity_sampleVideo (IntPtr session, int x, int y, int width, int height);

		delegate void internal_finishCallBack (bool success);

		[DllImport ("__Internal")]
		private static extern bool unity_stopRecord (IntPtr session, internal_finishCallBack finishCallBack);

		[DllImport ("__Internal")]
		private static extern bool unity_releaseRecordSession (IntPtr session);

		delegate void internal_movie_imageCallBack ();

		[DllImport ("__Internal")]
		private static extern bool unity_movieToImage (string file, string file1, internal_movie_imageCallBack callback);

		[DllImport ("__Internal")]
		private static extern bool unity_bindView ();

		public static void bindView ()
		{
			#if UNITY_IOS && !UNITY_EDITOR
		unity_bindView ();
			#endif
		}

		public static IEnumerator StartRecord (int x, int y, int width, int height, int deWidth, int deHeight, int fps, Action errorCB)
		{
			if (__recording) {
				Debug.Log ("Recording, do not start again");
				if (errorCB != null)
					errorCB ();
				yield return null;
			}
			try {
				_session = unity_createRecordSession ();
			} catch (Exception e) {
				Debug.Log ("CreateRecord, Exception:" + e.ToString ());
				if (errorCB != null)
					errorCB ();
			}
			errorCallBack = errorCB;
			bool audio_record = unity_preAudio (_session, _audioEncodeErrorCallBack);
			if (!audio_record) {
				Debug.Log ("Recording audio error, do not start");
				if (errorCB != null)
					errorCB ();
				yield return null;
			}
			bool video_record = unity_preVideo (_session, deWidth, deHeight, fps, _videoEncodeErrorCallBack);
			if (!video_record) {
				Debug.Log ("Recording video error, do not start");
				if (errorCB != null)
					errorCB ();
				yield return null;
			}
			__recording = true;
			__inter_recording = true;

			mediaAllPath = Application.persistentDataPath + "/" + Tools.generateStr (10) + ".mp4";
			bool started = unity_startRecord (_session, mediaAllPath);
			if (!started) {
				Debug.Log ("Recording start error");
				if (errorCB != null)
					errorCB ();
				yield return null;
			}
			__x = x;
			__y = y;
			__width = width;
			__height = height;
			while (__inter_recording) {
				yield return new WaitForFixedUpdate ();
				unity_sampleVideo (_session, x, y, width, height);
			}
			yield return null;
		}

		public static void Sample ()
		{
			if (!__inter_recording)
				unity_sampleVideo (_session, __x, __y, __width, __height);
		}

		[MonoPInvokeCallback (typeof(internal_audioEncodeErrorCallBack))]
		public static void _audioEncodeErrorCallBack ()
		{
			__inter_recording = false;
			if (errorCallBack != null)
				errorCallBack ();
		}

		[MonoPInvokeCallback (typeof(internal_videoEncodeErrorCallBack))]
		public static void _videoEncodeErrorCallBack ()
		{
			__inter_recording = false;
			if (errorCallBack != null)
				errorCallBack ();
		}

		public static void CancelRecord (Action CB)
		{
			if (!__recording) {
//			Debug.Log ("Record not start");
				CB ();
				return;
			}
			//		bool error = false;
			if (!__inter_recording) {
				Debug.Log ("Record is error");
				CB ();
				return;
			}
			__inter_recording = false;
			stop ((success) => {
				release ();
				CB ();
			});
		}

		public static void StopRecord (Action<bool, string, string> CB)
		{
			if (!__recording) {
				Debug.Log ("Record not start");
				CB (false, null, "Record not start");
				return;
			}
			bool error = false;
			if (!__inter_recording) {
				Debug.Log ("Record is error");
				error = true;
			}
			stop ((success) => {
				release ();
				if (!success || error) {
					CB (false, null, "Record is error");
				} else {
					CB (true, mediaAllPath, "success");
				}
			});
		}

		private static void stop (Action<bool> finishCB)
		{
			finishCallBack = finishCB;
			unity_stopRecord (_session, _finishCallBack);
		}

		[MonoPInvokeCallback (typeof(internal_finishCallBack))]
		public static void _finishCallBack (bool success)
		{
			if (!success)
				File.Delete (mediaAllPath);
			if (finishCallBack != null)
				finishCallBack (success);
		}

		private static void release ()
		{
			unity_releaseRecordSession (_session);
			//		_session = null;
			__inter_recording = false;
			__recording = false;
		}
	}
}
