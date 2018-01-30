using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace Gj
{
	public class Auth
	{
		public enum Platform {
			GUEST,
			GAMECENTER,
			QQ,
			WEIXIN,
			FACEBOOK
		}

		public class PlayerInfo
		{
			public string identity;
			public string name;
			public string platform;
		}

		public void GameCenter (Action<bool, PlayerInfo, string> CB)
		{
			Social.localUser.Authenticate ((success) => {
				if (success) {
					PlayerInfo play = new PlayerInfo ();
					play.platform = Platform.GAMECENTER.ToString();
					play.identity = Social.localUser.id;
					play.name = Social.localUser.userName;
					CB(true, play, "success");
				} else {
					CB(false, null, "gameCenter auth error!");
				}
			});	
		}

		public void Guest (Action<bool, PlayerInfo, string> CB)
		{
			PlayerInfo play = new PlayerInfo ();
			play.platform = Platform.GUEST.ToString();
			play.identity = DateTime.Now.ToFileTimeUtc().ToString();
			play.name = "";
			CB(true, play, "success");
		}

		public void QQ (Action<bool, PlayerInfo, string> CB) {
			UMAuth (Platform.QQ, CB);
		}

		public void WeiXin (Action<bool, PlayerInfo, string> CB) {
			UMAuth (Platform.WEIXIN, CB);
		}

		public void FaceBook (Action<bool, PlayerInfo, string> CB) {
			UMAuth (Platform.FACEBOOK, CB);
		}

		private void UMAuth (Platform platform, Action<bool, PlayerInfo, string> CB) {
			UM.Platform umPlatform;
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
				CB (false, null, "platform is error!");
				return;
			}
			UM.Authorize (umPlatform, (platformId, code, data) => {
				if (code == UM.SUCCESS) {
					PlayerInfo play = new PlayerInfo ();
					play.platform = platform.ToString();
					play.identity = data["uid"];
					play.name = data["name"];
					CB(true, play, "success");
				} else {
					CB(false, null, data["message"]);
				}
			});
		}
	}
}