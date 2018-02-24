using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;

namespace Gj.Galaxy.Logic{
    internal class ChatEvent
    {
        public const byte Message = 1;
    }
    public interface ChatDelegate
    {

    }
    // 聊天操作
    public class ChatConnect : NamespaceListener
    {
        private static Namespace n;
        public static AuthDelegate d;
        private static ChatConnect listener;

        static ChatConnect(){
            n = PeerClient.Of(NamespaceId.Chat);
            listener = new ChatConnect();
            n.listener = listener;
        }

        public void OnConnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public void OnDisconnect()
        {
            throw new System.NotImplementedException();
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public void OnEvent()
        {
            throw new System.NotImplementedException();
        }

        public void OnReconnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            throw new System.NotImplementedException();
        }
    }
}

