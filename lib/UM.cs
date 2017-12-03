using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Gj
{
	public class UM
	{


		//分享平台枚举

		public enum Platform : int
		{
			QQ = 0,
			SINA = 1,
			WEIXIN = 2,
			WEIXIN_CIRCLE = 3,
			QZONE = 4,
			EMAIL = 5,
			SMS = 6,
			FACEBOOK = 7,
			TWITTER = 8,
			WEIXIN_FAVORITE = 9,
			GOOGLEPLUS = 10,
			RENREN = 11,
			TENCENT = 12,
			DOUBAN = 13,
			FACEBOOK_MESSAGER = 14,
			YIXIN = 15,
			YIXIN_CIRCLE = 16,
			INSTAGRAM = 17,
			PINTEREST = 18,
			EVERNOTE = 19,
			POCKET = 20,
			LINKEDIN = 21,
			FOURSQUARE = 22,
			YNOTE = 23,
			WHATSAPP = 24,
			LINE = 25,
			FLICKR = 26,
			TUMBLR = 27,
			ALIPAY = 28,
			KAKAO = 29,
			DROPBOX = 30,
			VKONTAKTE = 31,
			DINGTALK = 32,
			MORE = 33}

		;

		//成功状态码
		//用于 授权回调 和 分享回调 的是非成功的判断
		public const int SUCCESS = 200;

		//授权回调

		public delegate void AuthDelegate (Platform platform, int stCode, Dictionary<string,string> message);

		//分享回调
		//注意 android 分享失败 没有 errorMsg
		public delegate void ShareDelegate (Platform platform, int stCode, string errorMsg);

		public delegate void ShareBoardDismissDelegate ();
		//授权某社交平台
		//platform 平台名 callback 授权成功完成
		public static void Authorize (Platform platform, AuthDelegate callback = null)
		{

			#if UNITY_ANDROID
			try 
			{

			SetPlatforms(new Platform[] { platform });
			Run(delegate
			{
			var androidAuthListener = new AndroidAuthListener(callback);
			UMSocialSDK.CallStatic("authorize", (int)platform, androidAuthListener);
			});
			}
			catch(AndroidJavaException exp)
			{
			Debug.LogError(exp.Message);
			}

			#elif UNITY_IOS && !UNITY_EDITOR
			authDelegate = callback;
			authorize((int)platform,AuthCallback);
			#endif
		}

		//解除某平台授权
		//platform 平台名 callback 解除完成回调
		public static void DeleteAuthorization (Platform platform, AuthDelegate callback = null)
		{

			#if UNITY_ANDROID
			try 
			{
			Run(delegate
			{
			var androidAuthListener = new AndroidAuthListener(callback);
			UMSocialSDK.CallStatic("deleteAuthorization", (int)platform, androidAuthListener);
			}
			);
			}
			catch(AndroidJavaException exp)
			{
			Debug.LogError(exp.Message);
			}

			#elif UNITY_IOS && !UNITY_EDITOR
			authDelegate = callback;
			deleteAuthorization((int)platform,AuthCallback);
			#endif

		}

		//打开分享面板
		//platforms 需要分享的平台数组 ,text 分享的文字, imagePath 分享的照片文件路径, callback 分享成功或失败的回调
		//imagePath可以为url 但是url图片必须以http://或者https://开头
		//imagePath如果为本地文件 只支持 Application.persistentDataPath下的文件
		//例如 Application.persistentDataPath + "/" +"你的文件名"
		//如果想分享 Assets/Resouces的下的 icon.png 请前使用 Resources.Load() 和 FileStream 写到 Application.persistentDataPath下
		public static void OpenShareWithImagePath (Platform[] platforms, string text, string imagePath, string title, string targeturl, ShareDelegate callback = null)
		{


			if (platforms == null) {
				Debug.LogError ("平台不能为空");
				return;
			}
			//var _platforms = platforms ?? Enum.GetValues(typeof(Platform)) as Platform[];
			var length = platforms.Length;
			var platformsInt = new int[length];
			for (int i = 0; i < length; i++) {
				platformsInt [i] = (int)platforms [i];

			}

			#if UNITY_ANDROID
			try 
			{

			Run(delegate
			{
			var androidShareListener = new AndroidShareListener(callback);
			UMSocialSDK.CallStatic("openShareWithImagePath", platformsInt, text, imagePath,  title, targeturl, androidShareListener);
			});

			}
			catch(AndroidJavaException exp)
			{
			Debug.LogError(exp.Message);
			}

			#elif UNITY_IOS && !UNITY_EDITOR
			shareDelegate = callback;
			openShareWithImagePath(platformsInt, length,text,imagePath,title,targeturl, ShareCallback);
			#endif
		}

		public static void setDismissDelegate (ShareBoardDismissDelegate callback = null)
		{
			#if UNITY_ANDROID
			try 
			{

			Run(delegate
			{
			var AndroidDismissListener = new AndroidDismissListener(callback);
			UMSocialSDK.CallStatic("setDismissCallBack",AndroidDismissListener);
			});

			}
			catch(AndroidJavaException exp)
			{
			Debug.LogError(exp.Message);
			}

			#elif UNITY_IOS && !UNITY_EDITOR
			dismissDelegate = callback;
			setDismissCallback(ShareBoardCallback);
			#endif

		}

		//直接分享到各个社交平台
		//platform 平台名，text 分享的文字，imagePath 分享的照片文件路径，callback 分享成功或失败的回调

		public static void DirectShare (Platform platform, string text, string imagePath, string title, string targeturl, string videourl, ShareDelegate callback = null)
		{
			try {

				#if UNITY_ANDROID

				// SetPlatforms(new Platform[] { platform });

				Run(delegate
				{
				var androidShareListener = new AndroidShareListener(callback);
				UMSocialSDK.CallStatic("directShare", text, imagePath,title,targeturl, (int)platform, androidShareListener);
				});

				#elif UNITY_IOS && !UNITY_EDITOR
				shareDelegate = callback;
				directShare( text, imagePath, title,targeturl,videourl,(int)platform, ShareCallback);
				#endif

			} catch (Exception e) {
				Debug.LogError (e.Message);
			}

		}

		//是否已经授权某平台
		//platform 平台名
		public static bool IsAuthorized (Platform platform)
		{
			#if UNITY_ANDROID

			return UMSocialSDK.CallStatic<bool>("isAuthorized", (int)platform);
			#elif UNITY_IOS && !UNITY_EDITOR

			return isAuthorized((int)platform);
			#else 
			return false;
			#endif
		}

		#if UNITY_ANDROID

		//设置SDK支持的平台
		public static void SetPlatforms(Platform[] platforms)
		{

		var length = platforms.Length;
		var platformsInt = new int[length];
		for (int i = 0; i < length; i++)
		{
		platformsInt[i] = (int)platforms[i];

		}

		Run(delegate
		{

		UMSocialSDK.CallStatic("setPlatforms", platformsInt);
		});

		}

		#endif

		//以下代码是内部实现
		//请勿修改

		#if UNITY_ANDROID


		delegate void Action();
		static void Run(Action action)
		{
		activity.Call("runOnUiThread", new AndroidJavaRunnable(action));
		}


		class AndroidAuthListener : AndroidJavaProxy
		{


		public AndroidAuthListener(AuthDelegate Delegate)
		: base("com.umeng.socialsdk.AuthListener")
		{
		this.authDelegate = Delegate;
		}

		AuthDelegate authDelegate = null;
		public void onAuth(int platform, int stCode, string key,string value)
		{
		Debug.Log("xxxxxx stCode="+stCode);
		string[] keys = key.Split(',');
		string[] values =value.Split(',');
		Dictionary<string, string> dic = new Dictionary<string, string>();
		//dic.Add (keys , values );
		for (int i = 0; i < keys.Length; i++) {
		dic.Add (keys [i], values [i]);
		}
		Debug.Log("xxxxxx length="+values.Length);

		authDelegate((Platform)platform, stCode, dic);
		}
		}

		class AndroidShareListener : AndroidJavaProxy
		{

		ShareDelegate shareDelegate = null;
		public AndroidShareListener(ShareDelegate Delegate)
		: base("com.umeng.socialsdk.ShareListener")
		{
		this.shareDelegate = Delegate;
		}
		public void onShare(int platform, int stCode, string errorMsg)
		{
		shareDelegate((Platform)platform, stCode, errorMsg);
		}
		}
		class AndroidDismissListener : AndroidJavaProxy
		{

		ShareBoardDismissDelegate shareDelegate = null;
		public AndroidDismissListener(ShareBoardDismissDelegate Delegate)
		: base("com.umeng.socialsdk.ShareBoardDismissListener")
		{
		this.shareDelegate = Delegate;
		}
		public void onDismiss()
		{

		shareDelegate();
		}
		}

		static AndroidJavaClass UMSocialSDK = new AndroidJavaClass("com.umeng.socialsdk.UMSocialSDK");

		static AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

		#endif

		public static string appKey = null;

		static AuthDelegate authDelegate = null;

		static ShareDelegate shareDelegate = null;
		static ShareBoardDismissDelegate dismissDelegate = null;
		//delegate void CallBack(IntPtr param);
		public delegate void AuthHandler (Platform platform, int stCode, string key, string value);

		[AOT.MonoPInvokeCallback (typeof(AuthHandler))]
		static void AuthCallback (Platform platform, int stCode, string key, string value)
		{
			Debug.Log ("xxxxxx stCode=" + stCode);
			string[] keys = key.Split (',');
			string[] values = value.Split (',');
			Dictionary<string, string> dic = new Dictionary<string, string> ();
			//dic.Add (keys , values );
			for (int i = 0; i < keys.Length; i++) {
				dic.Add (keys [i], values [i]);
			}
			Debug.Log ("xxxxxx length=" + values.Length);

			if (authDelegate != null)
				authDelegate (platform, stCode, dic);
		}


		[AOT.MonoPInvokeCallback (typeof(ShareDelegate))]
		static void ShareCallback (Platform platform, int stCode, string errorMsg)
		{
			if (shareDelegate != null)
				shareDelegate (platform, stCode, errorMsg);
		}

		[AOT.MonoPInvokeCallback (typeof(ShareBoardDismissDelegate))]
		static void ShareBoardCallback ()
		{
			if (dismissDelegate != null)
				dismissDelegate ();
		}

		[DllImport ("__Internal")]
		static extern void authorize (int platform, AuthHandler callback);

		[DllImport ("__Internal")]
		static extern void deleteAuthorization (int platform, AuthHandler callback);

		[DllImport ("__Internal")]
		static extern bool isAuthorized (int platform);

		[DllImport ("__Internal")]
		static extern void openShareWithImagePath (int[] platform, int platformNum, string text, string imagePath, string title, string targeturl, ShareDelegate callback);

		[DllImport ("__Internal")]
		static extern void directShare (string text, string imagePath, string title, string targeturl, string videourl, int platform, ShareDelegate callback);

		[DllImport ("__Internal")]
		static extern void setDismissCallback (ShareBoardDismissDelegate callback);

	}
}