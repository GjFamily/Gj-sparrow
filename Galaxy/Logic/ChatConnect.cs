using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;

namespace Gj.Galaxy.Logic{
    internal class ChatEvent
    {
        public const byte Message = 0;
    }
    // 聊天操作
    public class ChatConnect : NetworkListener, NamespaceListener
    {
        private static Namespace n;
        private static ChatConnect listener;
        public static ChatConnect Listener
        {
            get
            {
                return listener;
            }
        }

        static ChatConnect(){
            n = PeerClient.Of(NamespaceId.Chat);
            listener = new ChatConnect();
            n.listener = listener;
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            throw new System.NotImplementedException();
        }
    }
}

