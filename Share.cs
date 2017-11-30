using System;

namespace Gj
{
	public class Share
	{
		public enum Platform {
			SINA = "SINA",
			WEIXIN = "WEIXIN",
			FACEBOOK = "FACEBOOK"
		}

		public class ShareObject {
			public string text;
			public string imagePath;
			public string title;
			public string targeturl;
			public string videourl;
		}

		public void sina (ShareObject shareObject, Action<bool, string> CB) {
			UMShare(Platform.SINA, shareObject, CB);
		}

		public void weiXin (ShareObject shareObject, Action<bool, string> CB) {
			UMShare(Platform.WEIXIN, shareObject, CB);
		}

		public void faceBook (ShareObject shareObject, Action<bool, string> CB) {
			UMShare(Platform.FACEBOOK, shareObject, CB);
		}

		private void UMShare (Platform platform, ShareObject shareObject, Action<bool, string> CB) {
			int umPlatform;
			switch(platform) {
			case Platform.SINA:
				umPlatform = UM.Platform.QQ;
				break;
			case Platform.WEIXIN:
				umPlatform = UM.Platform.WEIXIN;
				break;
			case Platform.FACEBOOK:
				umPlatform = UM.Platform.FACEBOOK;
				break;
			default:
				CB (false, "platform is error!");
			}
			UM.DirectShare (umPlatform, shareObject.text, shareObject.imagePath, shareObject.title, shareObject.targeturl, shareObject.videourl, (platformId, code, message) => {
				if (code == UM.SUCCESS) {
					CB(true, "success!");
				} else {
					CB(false, message);
				}
			});
		}
	}
}