using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Gj
{
	
	public class Analysis
	{
		/// <summary>
		/// Source9 到Source 20 请在友盟后台网站设置 对应的定义
		/// </summary>
		public enum PaySource
		{
			AppStore = 1,
			支付宝 = 2,
			网银 = 3,
			财付通 = 4,
			移动 = 5,
			联通 = 6,
			电信 = 7,
			Paypal = 8,
			Source9,
			Source10,
			Source11,
			Source12,
			Source13,
			Source14,
			Source15,
			Source16,
			Source17,
			Source18,
			Source19,
			Source20
		}

		/// <summary>
		/// Source4 到Source 10 请在友盟后台网站设置 对应的定义
		/// </summary>
		public enum BonusSource
		{

			玩家赠送 = 1,
			Source2 = 2,
			Source3 = 3,
			Source4,
			Source5,
			Source6,
			Source7,
			Source8,
			Source9,
			Source10
		}

		public static void Event (string eventName)
		{
			#if UNITY_IOS && !UNITY_EDITOR
			analyticsEvent (eventName);
			#endif
		}

		public static void Recharge (int money, int price)
		{
			#if UNITY_IOS && !UNITY_EDITOR
			analyticsPayCashForCoin (Double.Parse(money.ToString()), (int)PaySource.AppStore, Double.Parse(price.ToString()));
			#endif
		}

		public static void Buy (string tag, int money)
		{
			#if UNITY_IOS && !UNITY_EDITOR
			analyticsBuy (tag, 1, money);
			#endif
		}

		[DllImport ("__Internal")]
		private static extern void analyticsEvent (string eventId);

		[DllImport ("__Internal")]
		private static extern void analyticsPayCashForCoin (double cash, int source, double coin);

		[DllImport ("__Internal")]
		private static extern void analyticsPayCashForItem (double cash, int source, string item, int amount, double price);

		[DllImport ("__Internal")]
		private static extern void analyticsBuy (string item, int amount, double price);
	}
}
