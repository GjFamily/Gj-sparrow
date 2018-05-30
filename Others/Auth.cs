using System;
using UnityEngine;

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
	}
}