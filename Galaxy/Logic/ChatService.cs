using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;

namespace Gj.Galaxy.Logic{
	internal class ChatEvent
	{
		public const byte Subscribe = 1;
		public const byte UnSubscribe = 2;
		public const byte Publish = 3;
    }


    // 聊天操作
	public class ChatService : NetworkListener, ServiceListener
    {
		private static ChatService listener;
		private static CometProxy proxy;
		const string PrivateMessage = "private";
		public static ChatService Listener
        {
            get
            {
                return listener;
            }
        }

		public delegate void OnPublishDelegate(string topic, string message, string target);
		public OnPublishDelegate OnPublish;

		private static Dictionary<string, string> topics = new Dictionary<string, string>();
		private static Dictionary<string, string> aliasList = new Dictionary<string, string>();

		static ChatService(){
			listener = new ChatService();
			proxy = PeerClient.Register(2, listener);
			listener.OnConnectEvent += (bool success) => {
				if (success)
				{
					var e = aliasList.GetEnumerator();
					while(e.MoveNext())
					{
						var v = e.Current;

						Subscribe(v.Value, v.Key);
					}
				}

			};
        }

		public ChatService()
		{
			
		}

        public static void Join()
		{
			if (proxy.IsConnected) return;

			proxy.Join();
		}

        public static void Leave()
		{
            topics.Clear();
            aliasList.Clear();
			proxy.Leave();
		}      

        public static void Subscribe(string topic, string alias)
		{
			topics[topic] = alias;
			aliasList[alias] = topic;
			proxy.Send(ChatEvent.Subscribe, topic.GetBytes());
		}

        public void UnSubscribe(string topic)
		{
			string t = null;
			var result = aliasList.TryGetValue(topic, out t);
			if (result) {
				aliasList.Remove(topic);
				topics.Remove(t);
				topic = t;
			} else {
				topics.Remove(topic);
			}
			proxy.Send(ChatEvent.UnSubscribe, topic.GetBytes());
		}

        public void Publish(string topic, string message)
		{
			proxy.SendObject(ChatEvent.Publish, new object[] { topic, message });
		}

        public void OnAccept(byte de, Message message)
		{
			object[] param;
			switch(de)
			{
				case ChatEvent.Subscribe:
					param = message.ReadObject();
					break;
				case ChatEvent.UnSubscribe:
					param = message.ReadObject();
					break;
				case ChatEvent.Publish:
					param = message.ReadObject();
					proxy.AcceptQueue(de, param);
					break;
				default:
					Debug.LogError(string.Format("Chat code is error:{0}", de));
					break;
			}
		}

        public void OnAcceptQueue(byte de, object[] param)
		{
			switch (de)
            {
                case ChatEvent.Subscribe:
                    break;
                case ChatEvent.UnSubscribe:
                    break;
                case ChatEvent.Publish:
					OnPublish((string)param[0], (string)param[1], (string)param[2]);
                    break;
                default:
                    Debug.LogError(string.Format("Chat code is error:{0}", de));
                    break;
            }
		}

		public void Update()
		{
		}
	}
}

