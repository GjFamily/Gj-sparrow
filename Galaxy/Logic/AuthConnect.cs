using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System;
using System.Collections.Generic;

namespace Gj.Galaxy.Logic{
    internal class AuthEvent
    {
        public const byte Auth = 0;
        public const byte Version = 1;
        public const byte User = 2;
    }
    // Auth操作
    public class AuthConnect : NetworkListener, NamespaceListener
    {
        private static Namespace n;
        private static AuthConnect listener;
        public static AuthConnect Listener
        {
            get
            {
                return listener;
            }
        }

        private Action<bool> OnConnectAction;

        static AuthConnect()
        {
            n = PeerClient.Of(NamespaceId.Auth);
            listener = new AuthConnect();
            n.listener = listener;
            listener.OnConnectEvent += (success) =>
            {
                if (listener.OnConnectAction != null) Listener.OnConnectAction(success);
            };
        }

        public static void App(Action<bool> a){
            listener.OnConnectAction = a;
            n.Connect();
        }

        public static void Auth(string userName, string password, Action<object> callback){
			n.Emit(AuthEvent.Auth, new object[] { userName, password }, (object[] obj) =>
			{
				callback(obj[0]);
				if ((string)obj[0] != "")
					PeerClient.SetToken((string)obj[0]);
			});
        }

        public static void Version(Action<string> callback){
            n.Emit(AuthEvent.Version, new object[] { }, (object[] obj) => callback((string)obj[0]));
        }

        public static void User(string userId, Action<Dictionary<string, object>> callback){
            n.Emit(AuthEvent.User, new object[] { userId }, (object[] obj) => callback((Dictionary<string, object>) obj[0]));
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            return null;
        }
    }
}

