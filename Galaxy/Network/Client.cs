using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Snappy;
using System.IO.Compression;
using MsgPack.Serialization;

namespace Gj.Galaxy.Network{
    public delegate long TimestampDelegate();
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Reconnecting
    }
    public enum LogLevel
    {
        Error,
        Info,
        Debug
    }
    internal enum ExitStatus
    {
        _,
        Client,
        Server
    }

    public interface ClientListener{
        void OnConnect(bool success);
        void OnReconnect(bool success);
        void OnDisconnect();
    }

    internal class QueueData{
        public Namespace ns;
        public NsData data;
    }

    public class Client
    {
        public ClientListener listener;
        public ConnectionState State = ConnectionState.Disconnected;
        protected string sid;
        private AppPacket app;
        protected Namespace root;
        private ExitStatus exitStatus = ExitStatus._;

        private Queue<QueueData> messageQueue = new Queue<QueueData>();

        protected string mWebSocket = "";
        protected IPEndPoint mTcp;
        protected IPEndPoint mUdp;

        protected Dictionary<ProtocolType, ProtocolConn> allowConn = new Dictionary<ProtocolType, ProtocolConn>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> readerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> writerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        static Client(){
            readerHandle[CompressType.Snappy] = (Stream arg1) =>
            {
                return new SnappyStream(arg1, CompressionMode.Decompress);
            };
            writerHandle[CompressType.Snappy] = (Stream arg1) =>
            {
                return new SnappyStream(arg1, CompressionMode.Compress);
            };
        }

        public bool IsConnect
        {
            get
            {
                switch (State)
                {
                    case ConnectionState.Connected:
                        //Debug.Log(true);
                        return true;
                    default:
                        return false;
                }
            }
        }

        public long ServerTimestamp;

        public TimestampDelegate LocalTimestamp;

        public int PingTime;

        public LogLevel logLevel = LogLevel.Error;

        public Client()
        {
            root = new Namespace(this, null);
        }

        public void SetApp(string appId, string version, string secret)
        {
            app = new AppPacket(appId, version, secret);
        }

        public void SetCrypto()
        {

        }

        public bool Connect(string url)
        {
            if (app == null)
                throw new Exception("Please set app info");
            if (IsConnect) return true;
            var result = WebSocket(url);
            if (result){
                Send(MessageType.Application, ProtocolType.Default, CompressType.None, app);
            }
            return result;
        }

        public void Disconnect()
        {
            if(IsConnect){
                SendByte(MessageType.Close, ProtocolType.Default, CompressType.None, null);

                destroy("forced client close");
            }
            State = ConnectionState.Disconnecting;
            exitStatus = ExitStatus.Client;
        }

        public bool Reconnect()
        {
            if (exitStatus != ExitStatus._)
            {
                return false;
            }
            if (allowConn.Keys.Count > 0)
            {
                return true;
            }
            State = ConnectionState.Reconnecting;
            return webSocket(mWebSocket);
        }

        public void Close()
        {
            Disconnect();
        }

        public bool WebSocket(string url)
        {
            var result = webSocket(url);
            if (result) mWebSocket = url;
            return result;
        }

        protected bool webSocket(string url)
        {
            Uri uri = new Uri(url + "/galaxy.socket");
            var conn = new WebSocket(uri);
            return Accept(ProtocolType.Safe, conn);
        }

        protected bool Tcp(string host, int port)
        {
            IPAddress address;
            IPAddress.TryParse(host, out address);
            var t = new IPEndPoint(address, port);
            var result = tcp(t);
            if (result) mTcp = t;
            return result;
        }

        protected bool tcp(IPEndPoint point)
        {
            //var conn = null;
            //return Accept(ProtocolType.Default, conn);
            return true;
        }

        protected bool Udp(string host, int port)
        {
            IPAddress address;
            IPAddress.TryParse(host, out address);
            var t = new IPEndPoint(address, port);
            var result = udp(t);
            if (result) mTcp = t;
            return result;
        }

        protected bool udp(IPEndPoint point)
        {
            //var conn = null;
            //return Accept(ProtocolType.S, conn);
            return true;
        }

        protected bool Accept(ProtocolType protocolType, ProtocolConn conn)
        {
            ProtocolConn c;
            bool result = allowConn.TryGetValue(protocolType, out c);
            if(result){
                c.Close();
            }
            allowConn[protocolType] = conn;

            SendByte(MessageType.Open, ProtocolType.Default, CompressType.None, sid.GetBytes());
            //Debug.Log(conn);
            //Debug.Log("accept");
            State = ConnectionState.Connecting;
            return true;
        }

        public Namespace Of(byte ns)
        {
            return root.Of(ns);
        }

        public Namespace Root()
        {
            return root;
        }

        public bool WriteQueue(int times)
        {
            var t = 0;
            NsDataArray a = null;
            Namespace ns = null;
            CompressType compress = CompressType.None;
            ProtocolType protocol = ProtocolType.Default;
            List<NsData> l = new List<NsData>();
            do
            {
                t++;
                var data = messageQueue.Dequeue();
                if (data == null) return false;
                if (ns == null){
                    ns = data.ns;
                    compress = ns.compress;
                    protocol = ns.protocol;
                }
                if(a == null) a = new NsDataArray();
                if (ns != data.ns && l.Count > 0)
                {
                    a.data = l.ToArray();
                    l.Clear();
                    Send(MessageType.Namespace, protocol, compress, a);
                    a = new NsDataArray();
                }
                l.Add(data.data);
            } while (t<=times);
            if(l.Count > 0) Send(MessageType.Namespace, protocol, compress, a);
            return true;
        }

        public bool ReadQueue(int times)
        {
            var enumerator = allowConn.Values.GetEnumerator();
            var h = new byte[9];
            Stream buffer;
            var t = 0;
            var length = 0;
            var multiplier = 1;
            Debug.Log("Read");
            while (enumerator.MoveNext())
            {
                Debug.Log("Read Connect");
                var conn = enumerator.Current;
                while(true)
                {
                    t++;
                    if (t > times) return true;
                    Debug.Log("Read Connect message");
                    var b = conn.Read(9, out h);
                    if(b == null){
                        return false;
                    }
                    length = 0;
                    multiplier = 1;
                    var message = new Message();
                    buffer = new MemoryStream();
                    message.type = (MessageType)(h[0] >> 4);
                    var compressType = (CompressType)(h[0] & 0xf);
                    message.time = BitConverter.ToInt32(h, 1);

                    for (var i = 5; i < 9; i++)
                    {
                        length += (int)(h[i] & 127) * multiplier;
                        multiplier *= 128;
                        if (h[i] == 0)
                        {
                            break;
                        }
                    }
                    if (compressType != CompressType.None)
                    {
                        buffer = readerHandle[compressType](buffer);
                    }
                    message.reader = buffer;
                    ServerTimestamp = message.time;
                    dispatch(message);
                }
            }
            return false;
        }

        private bool Write(MessageType messageType, ProtocolConn conn, CompressType compressType, Action<Stream> handler)
        {
            var header = new byte[9];
            header[0] = (byte)((byte)messageType << 4 & 0xff | (byte)compressType & 0xff);
            byte[] intBytes = BitConverter.GetBytes(ServerTimestamp);
            Array.Copy(intBytes, 0, header, 1, intBytes.Length);

            Stream buffer = new MemoryStream();
            if(compressType != CompressType.None)
            {
                var w = writerHandle[compressType](buffer);
                handler(w);
            }
            else
            {
                handler(buffer);
            }

            var length = buffer.Length;
            for (var i = 5; length > 0;i++)
            {
                var b = length % 128;
                length = length / 128;

                if (length > 0) {
                    b = b | 128;
                }
                header[i] = (byte)(b & 0xff);
            }
            //Debug.Log(conn);
            //Debug.Log(header);
            //Debug.Log(buffer);

            return conn.Write(header, buffer);
        }

        public void Protocol(ProtocolType protocolTmp, ProtocolType protocolType)
        {
            SendByte(MessageType.Protocol, protocolTmp, CompressType.None, protocolType.Protocol().GetBytes());
        }

        public void Ping()
        {
            SendByte(MessageType.Ping, ProtocolType.Default, CompressType.None, null);
        }

        internal void Packet(NsData data, Namespace n)
        {
            if (n.messageQueue == MessageQueue.Off){
                CompressType compress = CompressType.None;
                ProtocolType protocol = ProtocolType.Default;
                if (n != null)
                {
                    compress = n.compress;
                    protocol = n.protocol;
                }
                var a = new NsDataArray();
                a.data = new NsData[] { data };
                Send(MessageType.Namespace, protocol, compress, a);
            }else{
                var p = new QueueData();
                p.ns = n;
                p.data = data;
                messageQueue.Enqueue(p);
            }
        }

        private ProtocolConn SelectConn(ProtocolType protocolType)
        {
            ProtocolConn conn;
            bool result = allowConn.TryGetValue(protocolType, out conn);
            if (!result)
            {
                var enumerator = allowConn.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var _conn = enumerator.Current;
                    conn = _conn.Value;
                    Protocol(_conn.Key, protocolType);
                }
            }
            return conn;
        }

        internal void Send(MessageType messageType, ProtocolType protocolType, CompressType compressType, DataPacket data)
        {
            //var p = new QueueData();
            //p.message = messageType;
            //p.protocol = protocolType;
            //p.compress = compressType;
            //p.data = data;
            //messageQueue.Enqueue(p);
            var conn = SelectConn(protocolType);

            Write(messageType, conn, compressType, data.Packet);
        }

        internal void SendByte(MessageType messageType, ProtocolType protocolType, CompressType compressType, byte[] data)
        {
            var conn = SelectConn(protocolType);
            Write(messageType, conn, compressType, (writer) =>
            {
                //Debug.Log(writer);
                //Debug.Log(data);
                if(data != null)
                    writer.Write(data, 0, data.Length);
            });
        }

        void dispatch(Message message)
        {
            string v;
            ServerTimestamp = message.time;
            switch (message.type)
            {
                case MessageType.Close:
                    OnClose();
                    break;
                case MessageType.Pong:
                    v = new BinaryReader(message.reader).ReadString();
                    OnPong(v);
                    break;
                case MessageType.Protocol:
                    v = new BinaryReader(message.reader).ReadString();
                    OnProtocol(v);
                    break;
                case MessageType.Namespace:
                    OnNamespace(message);
                    break;
                case MessageType.Speed:
                    OnSpeed();
                    break;
                default:
                    Debug.Log(string.Format("Client accept error type:{0}", message.type));
                    break;
            }
        }

        private void OnClose()
        {
            State = ConnectionState.Disconnected;
            destroy("server is close");
        }

        private void OnPong(string sid)
        {
            if (State == ConnectionState.Connecting){
                State = ConnectionState.Connected;
                listener.OnConnect(true);
            }else if(State == ConnectionState.Reconnecting){
                State = ConnectionState.Connected;
                listener.OnReconnect(true);
            }
                
            this.sid = sid;
        }

        private void OnProtocol(string url)
        {

        }

        private void OnNamespace(Message message)
        {
            var nsData = NsDataArray.Unpack(message.reader);
            for (var i = 0; i < nsData.Length; i++){
                var data = nsData[i];
                var n = root;
                if (data.nsp != null)
                {
                    foreach (byte ns in data.nsp)
                    {
                        n = n.Of(ns);
                    }
                }
                n.dispatch(data);
            }

        }

        private void OnSpeed()
        {

        }

        private void destroy(string reason)
        {
            var enumerator = allowConn.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var _conn = enumerator.Current;
                _conn.Close();
            }
        }
    }
}

