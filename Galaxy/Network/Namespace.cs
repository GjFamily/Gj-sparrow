using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Gj.Galaxy.Network
{
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

    public interface NamespaceListener:ClientListener{
        object[] OnEvent(byte eb, object[] param);
        void OnError(string message);
    }

    public class NamespaceId{
        public const byte Default = 0;
    }

    public class Namespace
    {
        public NamespaceListener listener;
        private ConnectionState state = ConnectionState.Disconnected;
        internal bool compress = false;
        internal ProtocolType protocol = ProtocolType.Default;
        //internal MessageQueue messageQueue = MessageQueue.Off;
        //internal byte ns;
        internal byte[] nsp;
        //internal byte[] parent;
        private Nebula client;
        private Dictionary<byte, Namespace> nss = new Dictionary<byte, Namespace>();

        private int ackId = 0;
        private Dictionary<int, Action<object[]>> callbacks = new Dictionary<int, Action<object[]>>();

        private string query;
        internal bool needConnect = false;

        public ConnectionState State
        {
            get{
                return state;
            }
        }

		public Namespace(Nebula client)
        {
            //this.parent = null;
            this.nsp = new byte[]{};
            //this.ns = 0;
            this.client = client;
            //Namespace(client, null, null);
        }

		public Namespace(Nebula client, byte[] nsp)
        {
            //this.parent = nsp;
            //this.ns = ns;
            this.nsp = nsp;
            this.client = client;
        }

        public Namespace Of(byte ns){
            if (ns == NamespaceId.Default) return this;
            Namespace n;
            bool result = nss.TryGetValue(ns, out n);
            if (result){
                return n;
            }

            List<byte> nspArray = new List<byte>();
            if (nsp != null)
            {
                for (int i = 0; i < nsp.Length; i++)
                {
                    nspArray.Add(nsp[i]);
                }
            }
            nspArray.Add(ns);
            n = new Namespace(client, nspArray.ToArray());
            nss.Add(ns, n);
            return n;
        }

        public Namespace With(byte[] nsp){
            if (nsp == null) return this;
            Namespace n = this;
            foreach(byte ns in nsp){
                n = n.Of(ns);
            }
            return n;
        }

        public void Connect(string query = null){
            this.query = query;
            var data = new NsData();
            if (state != ConnectionState.Reconnecting)
                state = ConnectionState.Connecting;
            data.type = DataType.Connect;
            if (query != null)
                data.data = "/?" + query;
            needConnect = true;
            packet(data);
        }

        internal void Reconnect(){
            if (state == ConnectionState.Disconnected || state == ConnectionState.Disconnecting)
                return;
            if (!needConnect)
                return;
            state = ConnectionState.Reconnecting;
            Connect(query);
            var enumerator = nss.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var n = enumerator.Current;
                n.Value.Reconnect();
            }
        }

        public void Disconnect(){
            var data = new NsData();
            state = ConnectionState.Disconnecting;
            data.type = DataType.Disconnect;
            packet(data);
            needConnect = false;
            destroy("client namespace disconnect");
        }

        public void Emit(byte eb, object[] param, Action<object[]> callback = null){
            var data = new NsData();
            if(callback != null){
                ackId++;
                callbacks[ackId] = callback;
                data.id = ackId;
            }
            data.type = DataType.Event;
            var d = new List<object>();
            d.Add(eb);
            if(param != null)
                d.AddRange(param);
            data.data = d.ToArray();
            packet(data);
        }

        public void Error(string message){
            var data = new NsData();
            data.type = DataType.Error;
            data.data = message;
            packet(data);
        }

        public void SetCompress(bool compress){
            this.compress = compress;
        }

        internal void ack(int id, object[] result){
            var data = new NsData();
            data.type = DataType.Ack;
            data.id = id;
            data.data = result;
            packet(data);
        }

        internal void dispatch(NsData data){

            //Debug.Log("[ SOCKET ] accept Namespace:" + BitConverter.ToString(data.nsp==null?new byte[]{} : data.nsp) + "," + data.type + "," + state);
            //try{
                switch (data.type)
                {
                    case DataType.Connect:
                        if (state == ConnectionState.Reconnecting)
                        {
                            state = ConnectionState.Connected;
                            this.listener.OnReconnect(true);
                        }
                        else
                        {
                            state = ConnectionState.Connected;
                            this.listener.OnConnect(true);
                        }
                        break;
                    case DataType.Disconnect:
                        state = ConnectionState.Disconnected;
                        this.listener.OnDisconnect();
                        needConnect = false;
                        break;
                    case DataType.Error:
                        if (state == ConnectionState.Connecting)
                            this.listener.OnConnect(false);
                        else if (state == ConnectionState.Reconnecting)
                            this.listener.OnConnect(false);
                        else
                            this.listener.OnError((string)data.data);
                        break;
                    case DataType.Event:
                        var l = (object[])data.data;
                        if (l.Length == 0)
                        {
                            this.Error("event is miss");
                        }
                        else
                        {
                            var e = l[0];
                            var ll = new List<object>(l);
                            ll.RemoveAt(0);
                            var r = this.listener.OnEvent((byte)l[0], ll.ToArray());
                            if (data.id > 0){
                                ack(data.id, r);
                            }
                        }
                        break;
                    case DataType.Ack:
                        Action<object[]> action;
                        var result = callbacks.TryGetValue(data.id, out action);
                        if (result)
                        {
                            action((object[])data.data);
                        }
                        else
                        {
                            this.Error("Ack is error, id is miss");
                        }
                        break;
                    case DataType.Protocol:
                        this.OnProtocol((string)data.data);
                        break;
                    default:
                        Debug.Log(string.Format("Namespace accept error type:{0}", data.type));
                        break;
                }
            //}catch(Exception e){
            //    Debug.LogException(e);
            //}
        }

        private bool CheckTypeMatch(ParameterInfo[] pArray, Type[] argTypes)
        {
            throw new NotImplementedException();
        }

        private void OnProtocol(string protocol){
            this.protocol = protocol.ToProtocol();
        }

        private void packet(NsData data){
            if(data.nsp == null || data.nsp.Length == 0){
                data.nsp = nsp;
            }
            //Debug.Log("[ SOCKET ] send Namespace:" + BitConverter.ToString(data.nsp) + "," + data.type + "," + state);
            client.Packet(data, this);
        }

        private void destroy(string reason){
            var enumerator = nss.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var n = enumerator.Current;
                n.Value.Disconnect();
            }
            //nss = null;
            //client.Root().With(parent).remove(ns);
        }

        //private void remove(byte ns)
        //{
        //    Namespace n;
        //    var result = nss.TryGetValue(ns, out n);
        //    if (result && n != null)
        //    {
        //        nss.Remove(ns);
        //    }
        //}
    }
}

