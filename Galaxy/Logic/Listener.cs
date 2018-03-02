using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;

namespace Gj.Galaxy.Logic{
    public class NetworkListener : ClientListener
    {
        public delegate void OnConnectDelegate(bool success);
        public delegate void OnReconnectDelegate(bool success);
        public delegate void OnDisconnectDelegate();

        public event OnConnectDelegate OnConnectEvent;
        public event OnReconnectDelegate OnReconnectEvent;
        public event OnDisconnectDelegate OnDisconnectEvent;

        public void OnConnect(bool success)
        {
            if (OnConnectEvent != null) OnConnectEvent(success);
        }

        public void OnDisconnect()
        {
            if (OnDisconnectEvent != null) OnDisconnectEvent();
        }

        public void OnReconnect(bool success)
        {
            if (OnReconnectEvent != null) OnReconnectEvent(success);
        }
    }
}
