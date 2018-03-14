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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gj.Galaxy.Network{
    public delegate long TimestampDelegate();
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Reconnecting,
        WaitReconnect
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
        private ConnectionState state = ConnectionState.Disconnected;
        public ConnectionState State{
            get
            {
                return state;
            }
        }
        protected string sid;
        private AppPacket app;
        protected Namespace root;
        private ExitStatus exitStatus = ExitStatus._;

        private Queue<QueueData> outQueue = new Queue<QueueData>();
        private Queue<QueueData> inQueue = new Queue<QueueData>();

        protected string mWebSocket = "";
        protected IPEndPoint mTcp;
        protected IPEndPoint mUdp;

        protected Dictionary<ProtocolType, ProtocolConn> allowConn = new Dictionary<ProtocolType, ProtocolConn>();
        protected Dictionary<ProtocolType, ProtocolConn> acceptConn = new Dictionary<ProtocolType, ProtocolConn>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> readerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> writerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        static Client(){
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                // use generated resolver first, and combine many other generated/custom resolvers
                MessagePack.Resolvers.GeneratedResolver.Instance,
                //MessagePack.UnsafeExtensions.UnityBlitResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,

                // finally, use builtin/primitive resolver(don't use StandardResolver, it includes dynamic generation)
                MessagePack.Resolvers.BuiltinResolver.Instance,
                MessagePack.Resolvers.AttributeFormatterResolver.Instance,
                MessagePack.Resolvers.PrimitiveObjectResolver.Instance
            );
            readerHandle[CompressType.Snappy] = (Stream arg1) =>
            {
                return new SnappyStream(arg1, CompressionMode.Decompress);
            };
            writerHandle[CompressType.Snappy] = (Stream arg1) =>
            {
                return new SnappyStream(arg1, CompressionMode.Compress);
            };
        }

        public bool IsConnected
        {
            get
            {
                switch (state)
                {
                    case ConnectionState.Connected:
                        //Debug.Log(true);
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsRuning
        {
            get
            {
                switch (state)
                {
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                    case ConnectionState.Reconnecting:
                    case ConnectionState.WaitReconnect:
                        //Debug.Log(true);
                        return true;
                    default:
                        return false;
                }
            }
        }

        public int ServerTimestamp;

        public TimestampDelegate LocalTimestamp;
        public long LastTimestamp;

        public int ReconnectTimes = 10;
        private int nowReconnectTimes = 0;
        private int ReconnectTimeout = 10; // seconds
        private long ReconnectLast = 0;

        private long sendLocalTimestamp;
        public int PingTime;

        public LogLevel logLevel = LogLevel.Error;

        public Client()
        {
            root = new Namespace(this);
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
            Debug.Log("connect");
            if (app == null)
                throw new Exception("Please set app info");
            if (IsConnected) return true;
            exitStatus = ExitStatus._;
            var result = WebSocket(url);
            return result;
        }

        public void Disconnect()
        {
            state = ConnectionState.Disconnecting;
            exitStatus = ExitStatus.Client;
            if(IsRuning){
                SendByte(MessageType.Close, ProtocolType.Default, CompressType.None, null);
            }
            destroy("forced client close");
        }

        public bool Reconnect()
        {
            if (!UpdateState())
            {
                return false;
            }
            Reconnect(ProtocolType.Default);
            Reconnect(ProtocolType.Safe);
            Reconnect(ProtocolType.Speed);

            return true;
        }

        internal bool UpdateState()
        {
            // 运行后执行，isRuning
            if (exitStatus != ExitStatus._)
            {
                return false;
            }
            // 运行后，才能确保是需要重连
            if (allowConn.Keys.Count == 0 && acceptConn.Keys.Count == 0)
            {
                if (state != ConnectionState.Reconnecting)
                    state = ConnectionState.WaitReconnect;
            }
            // 重连间隔
            if (LocalTimestamp() - ReconnectLast < ReconnectTimeout * 1000)
            {
                return false;
            }
            // 重连最大次数
            if (nowReconnectTimes > ReconnectTimes)
            {
                if (state == ConnectionState.Reconnecting && allowConn.Keys.Count == 0)
                {
                    listener.OnReconnect(false);
                }
                return false;
            }
            return true;
        }

        internal void ResetReconnect(){
            ReconnectLast = LocalTimestamp();
            nowReconnectTimes = 0;
            ReconnectTimeout = 1;
        }

        internal bool Reconnect(ProtocolType protocolType, bool force=false)
        {
            ProtocolConn conn;
            bool result;
            // 正在链接中
            result = acceptConn.TryGetValue(protocolType, out conn);
            if (result) return true;

            result = allowConn.TryGetValue(protocolType, out conn);
            if (result)
            {
                // 正常的链接
                if (conn.Connected()) return true;
                allowConn.Remove(protocolType);
            }
            Debug.Log("Reconnect:" + protocolType);
            ReconnectLast = LocalTimestamp();
            nowReconnectTimes++;
            ReconnectTimeout++;
            if (state == ConnectionState.WaitReconnect)
                state = ConnectionState.Reconnecting;
            // 开始重连
            switch(protocolType){
                default:
                case ProtocolType.Safe:
                    return webSocket(mWebSocket);
                case ProtocolType.Default:
                    if (mTcp != null)
                    return tcp(mTcp);
                    break;
                case ProtocolType.Speed:
                    if (mUdp != null)
                    return udp(mUdp);
                    break;
            }
            return false;
        }

        public void Close()
        {
            Disconnect();
        }

        //public void WaitConnect()
        //{
        //    var enumerator = acceptConn.GetEnumerator();
        //    ProtocolConn c;
        //    while (enumerator.MoveNext())
        //    {
        //        var current = enumerator.Current;
        //        var conn = current.Value;
        //        if (!conn.Connected()){
        //            var err = conn.Error();
        //            if (err == null) continue;
        //            Debug.LogError(err);
        //            return;
        //        }
        //        bool result = allowConn.TryGetValue(current.Key, out c);
        //        if (result)
        //        {
        //            c.Close();
        //        }
        //        acceptConn.Remove(current.Key);
        //        Debug.Log(conn);
        //        allowConn.Add(current.Key, conn);
        //        Debug.Log("send open");
        //        SendByte(MessageType.Open, ProtocolType.Default, CompressType.None, sid.GetBytes());
        //        return;
        //    }
        //}

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
            var result = IPAddress.TryParse(host, out address);
            if (!result) return false;
            var t = new IPEndPoint(address, port);
            if (tcp(t)) mTcp = t;
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
            var result = IPAddress.TryParse(host, out address);
            if (!result) return false;
            var t = new IPEndPoint(address, port);
            if (udp(t)) mTcp = t;
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
            Debug.Log("accept conn");

            var h = new byte[9];
            //Stream buffer;
            var length = 0;
            var multiplier = 1;
            acceptConn.Add(protocolType, conn);
            conn.Connect(()=>{
                try{
                    ResetReconnect();
                    acceptConn.Remove(protocolType);
                    allowConn.Add(protocolType, conn);
                    Debug.Log("[ SOCKET ] send open:" + sid);
                    //Debug.Log(sid.GetBytes().GetString());
                    SendByte(MessageType.Open, protocolType, CompressType.None, sid.GetBytes());
                }catch(Exception e){
                    Debug.LogException(e);
                }
            }, ()=>{
                Debug.Log("[ SOCKET ] accept close:"+protocolType);
                try{
                    acceptConn.Remove(protocolType);
                    allowConn.Remove(protocolType);
                    if(state == ConnectionState.Connecting){
                        listener.OnConnect(false);
                    }
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        return;
                    }
#endif
                    if(UpdateState())
                        Reconnect(protocolType);
                }catch(Exception e){
                    Debug.LogException(e);
                } 
            }, ()=>{
                try
                {
                    var buffer = conn.Read(9, out h);
                    if (buffer == null)
                    {
                        return;
                    }
                    length = 0;
                    multiplier = 1;
                    var message = new Message();
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
                    //Debug.Log(length);
                    var b = new byte[length];
                    buffer.Read(b, 0, length);

                    message.reader = new MemoryStream(b, false);
                    ServerTimestamp = message.time;
                    dispatch(message);
                }catch(Exception e){
                    Debug.LogException(e);
                }
            }, (e)=>{
                try
                {
                    Debug.LogException(e);
                    if (!conn.Connected())
                    {
                        Reconnect(protocolType);
                    }
                }
                catch (Exception ee)
                {
                    Debug.LogException(ee);
                }
            });
            if(state != ConnectionState.Connected)
                state = ConnectionState.Connecting;
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
            if (!IsConnected) return false;
            var t = 0;
            NsDataArray a = null;
            Namespace ns = null;
            CompressType compress = CompressType.None;
            ProtocolType protocol = ProtocolType.Default;
            List<NsData> l = new List<NsData>();
            do
            {
                //Debug.Log(outQueue.Count);
                if(outQueue.Count == 0){
                    break;
                }
                var data = outQueue.Dequeue();
                if (data == null) break;
                t++;
                if (ns == null){
                    ns = data.ns;
                    //compress = ns.compress;
                    //protocol = ns.protocol;
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
            if(l.Count > 0){
                a.data = l.ToArray();
                l.Clear();
                Send(MessageType.Namespace, protocol, compress, a);
            }
            return t == times;
        }

        public bool ReadQueue(int times)
        {
            var t = 0;
            do
            {
                t++;
                if(inQueue.Count == 0){
                    return false;
                }
                var data = inQueue.Dequeue();
                if (data == null) return false;
                data.ns.dispatch(data.data);
            } while (t <= times);
            return true;
        }

        private bool Write(MessageType messageType, ProtocolConn conn, CompressType compressType, Action<Stream> handler)
        {
            var header = new byte[9];
            header[0] = (byte)((byte)messageType << 4 & 0xff | (byte)compressType & 0xff);
            byte[] intBytes = BitConverter.GetBytes(ServerTimestamp);
            //Debug.Log(intBytes.Length);
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
            buffer.Position = 0;
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
            LastTimestamp = LocalTimestamp();

            return conn.Write(header, buffer);
        }

        public void Protocol(ProtocolType protocolTmp, ProtocolType protocolType)
        {
            switch(protocolType){
                case ProtocolType.Default:
                    if (mTcp != null) {
                        return;
                    }
                    break;
                case ProtocolType.Speed:
                    if (mUdp != null) {
                        return;
                    }
                    break;
                default:
                    Debug.Log("[ SOCKET ] unknow protocol");
                    break;
            }
            ProtocolConn conn;
            var result = acceptConn.TryGetValue(protocolType, out conn);
            if (result) return;
            acceptConn.Add(protocolType, null);
            Debug.Log("[ SOCKET ] send protocol:"+protocolType.Protocol());
            SendByte(MessageType.Protocol, protocolTmp, CompressType.None, protocolType.Protocol().GetBytes());
        }

        public void Ping()
        {
            Debug.Log("[ SOCKET ] send ping");
            sendLocalTimestamp = LocalTimestamp();
            //todo reconnect
            SendByte(MessageType.Ping, ProtocolType.Default, CompressType.None, null);
        }

        internal void Packet(NsData data, Namespace n)
        {
            if (n.messageQueue == MessageQueue.Off){
                CompressType compress = CompressType.None;
                ProtocolType protocol = ProtocolType.Default;
                //if (n != null)
                //{
                //    compress = n.compress;
                //    protocol = n.protocol;
                //}
                var a = new NsDataArray();
                a.data = new NsData[] { data };
                Send(MessageType.Namespace, protocol, compress, a);
            }else{
                var p = new QueueData();
                p.ns = n;
                p.data = data;
                outQueue.Enqueue(p);
                //Debug.Log(outQueue.Count);
            }
        }

        //internal void CheckConn()
        //{
        //    var enumerator = allowConn.GetEnumerator();
        //    ProtocolConn conn;
        //    while (enumerator.MoveNext())
        //    {
        //        var _conn = enumerator.Current;
        //        conn = _conn.Value;
        //        if (conn.Connected()) continue;
        //        Reconnect(_conn.Key);
        //    }
        //}

        private ProtocolConn SelectConn(ProtocolType protocolType)
        {
            ProtocolConn conn;
            bool flag = false;
            bool result = allowConn.TryGetValue(protocolType, out conn);
            if (!result) flag = true;
            if (conn == null || !conn.Connected())
            {
                Reconnect(protocolType);
                flag = true;
            }

            if(flag)
            {
                var enumerator = allowConn.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var _conn = enumerator.Current;
                    conn = _conn.Value;
                    if (!conn.Connected()) continue;
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
            if (conn == null)
            {
                return;
            }
            Write(messageType, conn, compressType, data.Packet);
        }

        internal void SendByte(MessageType messageType, ProtocolType protocolType, CompressType compressType, byte[] data)
        {
            var conn = SelectConn(protocolType);
            if(conn == null){
                return;
            }
            //if(data != null){
            //    Debug.Log(data.Length);
            //    Debug.Log(new StreamReader(new MemoryStream(data,0,data.Length)).ReadToEnd());  
            //}
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
                case MessageType.Open:
                    v = new StreamReader(message.reader).ReadToEnd();
                    Debug.Log("[ SOCKET ] accept open:" + v);
                    OnOpen(v);
                    break;
                case MessageType.Close:
                    Debug.Log("[ SOCKET ] accept close");
                    OnClose();
                    break;
                case MessageType.Pong:
                    Debug.Log("[ SOCKET ] accept pong");
                    OnPong();
                    break;
                case MessageType.Protocol:
                    v = new StreamReader(message.reader).ReadToEnd();
                    Debug.Log("[ SOCKET ] accept Protocol:" + v);
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
            state = ConnectionState.Disconnected;
            destroy("server is close");
        }
        private void OnOpen(string sid)
        {
            this.sid = sid;
            Send(MessageType.Application, ProtocolType.Default, CompressType.None, app);
            if (state == ConnectionState.Connecting)
            {
                state = ConnectionState.Connected;
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
                listener.OnReconnect(true);
                Root().Reconnect();
            }
            Ping();
        }
        private void OnPong()
        {
            if(sendLocalTimestamp > 0){
                PingTime = (int)((LocalTimestamp() - sendLocalTimestamp) / 2);
            }
        }

        private void OnProtocol(string url)
        {
            var u = new Uri(url);
            switch(u.Scheme.ToProtocol()){
                case ProtocolType.Default:
                    Tcp(u.Host, u.Port);
                    break;
                case ProtocolType.Speed:
                    Udp(u.Host, u.Port);
                    break;
                default:
                    Debug.LogError("[ SOCKET ] accept protocol is error");
                    break;
            }
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
                if (n.messageQueue == MessageQueue.Off)
                {
                    n.dispatch(data);
                }
                else
                {
                    var p = new QueueData();
                    p.ns = n;
                    p.data = data;
                    inQueue.Enqueue(p);
                }
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
            allowConn.Clear();
            acceptConn.Clear();
            state = ConnectionState.Disconnected;
        }
    }
}

