using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Gj.Galaxy.Network
{
    public interface NamespaceListener{
        void OnConnect(bool success);
        void OnDisconnect();
        void OnReconnect(bool success);
        object[] OnEvent(byte eb, object[] param);
        void OnError(string message);
    }

    public class Namespace
    {
        public NamespaceListener listener;
        public ConnectionState state = ConnectionState.Disconnected;
        internal CompressType compress = CompressType.None;
        internal ProtocolType protocol = ProtocolType.Default;
        internal MessageQueue messageQueue = MessageQueue.Off;
        internal byte[] nsp;
        private Client client;
        private Dictionary<byte, Namespace> nss = new Dictionary<byte, Namespace>();

        private int ackId = 0;
        private Dictionary<int, Action<object[]>> callbacks = new Dictionary<int, Action<object[]>>();

        private string query;
        private bool connecting;
        internal bool needConnect;

        public Namespace(Client client, byte[] nsp)
        {
            this.nsp = nsp;
            this.client = client;
        }

        public Namespace Of(byte ns){
            if (ns == 0) return this;
            Namespace n;
            bool result = nss.TryGetValue(ns, out n);
            if (result){
                return n;
            }
            List<byte> nsp = new List<byte>();
            for (int i = 0; i < this.nsp.Length; i++)
            {
                nsp.Add(this.nsp[i]);
            }
            n = new Namespace(client, nsp.ToArray());
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
                data.data = ("/?" + query).GetBytes();
            packet(data);
        }

        internal void Reconnect(){
            if (state == ConnectionState.Disconnected || state == ConnectionState.Disconnecting)
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

        public void SetCompressType(CompressType type){
            this.compress = type;
        }

        internal void dispatch(NsData data){
            try{
                switch (data.type)
                {
                    case DataType.Connect:
                        if (state == ConnectionState.Reconnecting)
                        {
                            this.listener.OnReconnect(true);
                        }
                        else
                        {
                            this.listener.OnConnect(true);
                        }
                        state = ConnectionState.Connected;
                        break;
                    case DataType.Disconnect:
                        this.listener.OnDisconnect();
                        state = ConnectionState.Disconnected;
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
                            this.listener.OnEvent((byte)l[0], ll.ToArray());
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
            }catch(Exception e){
                Debug.Log(e);
            }
        }

        private bool CheckTypeMatch(ParameterInfo[] pArray, Type[] argTypes)
        {
            throw new NotImplementedException();
        }

        private void OnProtocol(string protocol){
            this.protocol = protocol.ToProtocol();
        }

        private void packet(NsData data){
            if(data.nsp == null){
                data.nsp = nsp;
            }
            client.Packet(data, this);
        }

        private void destroy(string reason){
            var enumerator = nss.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var n = enumerator.Current;
                n.Value.Disconnect();
            }
            connecting = false;
        }
    }
}

