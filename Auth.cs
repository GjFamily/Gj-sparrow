using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace Gj
{
	public class Auth
	{
		public enum Platform {
			GUEST = "GUEST",
			GAMECENTER = "GAMECENTER",
			QQ = "QQ",
			WEIXIN = "WEIXIN",
			FACEBOOK = "FACEBOOK"
		}

		public class PlayerInfo
		{
			public string identity;
			public string name;
			public string platform;
		}

		public void gameCenter (Action<PlayerInfo> successCB, Action<String> errorCB)
		{
			Social.localUser.Authenticate ((success) => {
				if (success) {
					PlayerInfo play = new PlayerInfo ();
					play.platform = Platform.GAMECENTER;
					play.identity = Social.localUser.id;
					play.name = Social.localUser.userName;
					successCB(play);
				} else {
					errorCB("gameCenter auth error!");
				}
			});	
		}

		public void guest (Action<PlayerInfo> successCB, Action<String> errorCB)
		{
			PlayerInfo play = new PlayerInfo ();
			play.platform = Platform.GUEST;
			play.identity = DateTime.Now.ToFileTimeUtc();
			play.name = "";
			successCB(play);
		}

		public void qq (Action<PlayerInfo> successCB, Action<String> errorCB) {
			UMAuth (Platform.QQ, successCB, errorCB);
		}

		public void weiXin (Action<PlayerInfo> successCB, Action<String> errorCB) {
			UMAuth (Platform.WEIXIN, successCB, errorCB);
		}

		public void faceBook (Action<PlayerInfo> successCB, Action<String> errorCB) {
			UMAuth (Platform.FACEBOOK, successCB, errorCB);
		}

		private void UMAuth (Platform platform, Action<PlayerInfo> successCB, Action<String> errorCB) {
			int umPlatform;
			switch(platform) {
			case Platform.QQ:
				umPlatform = UM.Platform.QQ;
				break;
			case Platform.WEIXIN:
				umPlatform = UM.Platform.WEIXIN;
				break;
			case Platform.FACEBOOK:
				umPlatform = UM.Platform.FACEBOOK;
				break;
			default:
				errorCB ("platform is error!");
			}
			UM.Authorize (umPlatform, (platformId, code, data) => {
				if (code == UM.SUCCESS) {
					PlayerInfo play = new PlayerInfo ();
					play.platform = platform;
					play.identity = data["uid"];
					play.name = data["name"];
					successCB(play);
				} else {
					errorCB(data["message"]);
				}
			});
		}
	}
}