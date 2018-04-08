using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Snappy;
using System.IO.Compression;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gj.Galaxy.Network
{
    public delegate long TimestampDelegate();
    public delegate void OnDataLengthDelegate(long length);
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

    public interface ClientListener
    {
        void OnConnect(bool success);
        void OnReconnect(bool success);
        void OnDisconnect();
    }

    internal class QueueData
    {
        public Namespace ns;
        public NsData data;
    }

    public class Client
    {
        const int headLength = 6;
        public ClientListener listener;
        private ConnectionState state = ConnectionState.Disconnected;
        public ConnectionState State
        {
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

        public event OnDataLengthDelegate InData;
        public event OnDataLengthDelegate OutData;

        protected Dictionary<ProtocolType, ProtocolConn> allowConn = new Dictionary<ProtocolType, ProtocolConn>();
        protected List<ProtocolConn> updateConn = new List<ProtocolConn>();
        protected List<ProtocolType> acceptConn = new List<ProtocolType>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> readerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        protected static Dictionary<CompressType, Func<Stream, Stream>> writerHandle = new Dictionary<CompressType, Func<Stream, Stream>>();
        static Client()
        {
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

        public TimestampDelegate LocalTimestamp;
        // 服务端的时间戳
        public int ServerTimestamp;
        public long LastPingTimestamp;
        public long PingTime;
        // 最后次发送时间
        public long LastTimestamp;

        public int ReconnectTimes = 10;
        private int nowReconnectTimes = 0;
        private int ReconnectTimeout = 10; // seconds
        private long ReconnectLast = 0;

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
            Debug.Log("[ SOCKET ] connect");
            if (app == null)
                throw new Exception("Please set app info");
            if (IsRuning) return true;
            state = ConnectionState.Connecting;
            exitStatus = ExitStatus._;
            var result = WebSocket(url);
            return result;
        }

        public void Disconnect()
        {
            Debug.Log("[ SOCKET ] disconnect");
            state = ConnectionState.Disconnecting;
            exitStatus = ExitStatus.Client;
            if (IsRuning)
            {
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

        // 用于外部触发更新
        public void Update()
        {
            try
            {
                // 刷新需要更新的连接，避免刷新过程中变更
                if (updateConn.Count != allowConn.Count)
                {
                    updateConn.Clear();
                    updateConn.AddRange(allowConn.Values);
                }
                var enumerator = updateConn.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var _conn = enumerator.Current;
                    _conn.Update();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        internal bool UpdateState()
        {
            // 运行后执行，isRuning
            if (exitStatus != ExitStatus._)
            {
                return false;
            }
            // 运行后，才能确保是需要重连
            if (allowConn.Keys.Count == 0)
            {
                if (state == ConnectionState.Connected)
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

        internal void ResetReconnect()
        {
            ReconnectLast = LocalTimestamp();
            nowReconnectTimes = 0;
            ReconnectTimeout = 1;
        }

        internal bool Reconnect(ProtocolType protocolType, bool force = false)
        {
            ProtocolConn conn;
            bool result;
            result = allowConn.TryGetValue(protocolType, out conn);
            if (result)
            {
                // 正常的链接, 或者链接中
                if (conn.Connected || conn.Connecting) return true;
                allowConn.Remove(protocolType);
            }
            Debug.Log("[ SOCKET ] Reconnect:" + protocolType);
            ReconnectLast = LocalTimestamp();
            nowReconnectTimes++;
            ReconnectTimeout++;
            if (state == ConnectionState.WaitReconnect)
                state = ConnectionState.Reconnecting;
            // 开始重连
            switch (protocolType)
            {
                default:
                case ProtocolType.Safe:
                    if (mWebSocket != "")
                        return webSocket(mWebSocket);
                    break;
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

        public bool WebSocket(string url)
        {
            var result = webSocket(url);
            if (result) mWebSocket = url;
            return result;
            //IPAddress address;
            //var result = IPAddress.TryParse("127.0.0.1", out address);
            //var t = new IPEndPoint(address, 9001);
            //if (tcp(t)) mTcp = t;
            //return result;

            //IPAddress address;
            //var result = IPAddress.TryParse("127.0.0.1", out address);
            //if (!result) return false;
            //var t = new IPEndPoint(address, 54321);
            //if (udp(t)) mTcp = t;
            //return result;
        }

        protected bool webSocket(string url)
        {
            //Debug.Log(url);
            Uri uri = new Uri(url + "/galaxy.socket");
            var conn = new WebSocketAgent(uri);

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
            //var conn = new TcpSocket(point);
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
            var conn = new UdpSocket(point);
            return Accept(ProtocolType.Speed, conn);
            //return true;
        }

        protected bool Accept(ProtocolType protocolType, ProtocolConn conn)
        {
            if (allowConn.ContainsKey(protocolType)) allowConn.Remove(protocolType);
            allowConn.Add(protocolType, conn);
            //Debug.Log("[ SOCKET ] accept conn" + protocolType);

            var h = new byte[6];
            //Stream buffer;
            var length = 0;
            var multiplier = 1;
            conn.Accept(ref h, () =>
            {
                try
                {
                    length = 0;
                    multiplier = 1;
                    var message = new Message();
                    message.type = (MessageType)(h[0] >> 4);
                    var compressType = (CompressType)(h[0] & 0xf);
                    //message.time = BitConverter.ToInt32(h, 1);

                    for (var i = headLength - 4; i < headLength; i++)
                    {
                        length += (int)(h[i] & 127) * multiplier;
                        multiplier *= 128;
                        if (h[i] == 0)
                        {
                            break;
                        }
                    }
                    //Debug.Log(length);
                    var b = new byte[length];
                    conn.Read(ref b, () => {
                        var buffer = new MemoryStream(b, false);
                        //Debug.Log("compress:" + compressType);
                        if (compressType != CompressType.None)
                        {
                            message.reader = readerHandle[compressType](buffer);
                        }
                        else
                        {
                            message.reader = buffer;
                        }

                        //ServerTimestamp = message.time;
                        if (InData != null) InData(headLength + length);
                        dispatch(conn, protocolType, message);
                    });

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });

            // Begin Connect
            conn.Connect(() => {
                try
                {
                    //Debug.Log("[ SOCKET ] send open:" + sid);
                    //Debug.Log(sid.GetBytes().GetString());
                    SendByte(MessageType.Open, protocolType, CompressType.None, sid.GetBytes(), conn);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, () => {
                //Debug.Log("[ SOCKET ] accept close:" + protocolType);
                try
                {
                    allowConn.Remove(protocolType);
                    if (state == ConnectionState.Connecting)
                    {
                        listener.OnConnect(false);
                    }
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        return;
                    }
#endif
                    if (IsRuning && UpdateState())
                        Reconnect(protocolType);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, (e) => {
                try
                {
                    Debug.LogException(e);
                    if (!conn.Connected)
                    {
                        Reconnect(protocolType);
                    }
                }
                catch (Exception ee)
                {
                    Debug.LogException(ee);
                }
            });
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
                if (outQueue.Count == 0)
                {
                    break;
                }
                var data = outQueue.Dequeue();
                if (data == null) break;
                t++;
                if (ns == null)
                {
                    ns = data.ns;
                    compress = ns.compress;
                    protocol = ns.protocol;
                }
                if (a == null) a = new NsDataArray();
                if (ns != data.ns && l.Count > 0)
                {
                    a.data = l.ToArray();
                    l.Clear();
                    Send(MessageType.Namespace, protocol, compress, a);
                    a = new NsDataArray();
                }
                l.Add(data.data);
            } while (t <= times);
            if (l.Count > 0)
            {
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
                if (inQueue.Count == 0)
                {
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
            var header = new byte[headLength];
            header[0] = (byte)((byte)messageType << 4 & 0xff | (byte)compressType & 0xff);
            //byte[] intBytes = BitConverter.GetBytes(ServerTimestamp);
            //Debug.Log(intBytes.Length);
            //Array.Copy(intBytes, 0, header, 1, intBytes.Length);

            Stream buffer = new MemoryStream();
            if (compressType != CompressType.None)
            {
                var w = writerHandle[compressType](buffer);
                handler(w);
                w.Flush();
                //handler(buffer);
                //buffer.Position = 0;
                //Debug.Log(BitConverter.ToString(new BinaryReader(buffer).ReadBytes((int)buffer.Length)));
                //var r = readerHandle[compressType](buffer);
                //Stream buffer2 = new MemoryStream();
                //handler(buffer2);
                //buffer2.Position = 0;
                //var v = new BinaryReader(r).ReadBytes((int)buffer2.Length);
                //Debug.Log(BitConverter.ToString(v));
                //Debug.Log(BitConverter.ToString(new BinaryReader(buffer2).ReadBytes((int)buffer2.Length)));
            }
            else
            {
                handler(buffer);
            }
            buffer.Position = 0;
            var length = buffer.Length;

            //Debug.Log(length);
            for (var i = headLength - 4; length > 0; i++)
            {
                var b = length % 128;
                length = length / 128;

                if (length > 0)
                {
                    b = b | 128;
                }
                header[i] = (byte)(b & 0xff);
            }
            LastTimestamp = LocalTimestamp();
            if (OutData != null) OutData(headLength + buffer.Length);
            return conn.Write(header, buffer);
        }

        public void Protocol(ProtocolType protocolTmp, ProtocolType protocolType)
        {
            switch (protocolType)
            {
                case ProtocolType.Default:
                    if (mTcp != null)
                    {
                        return;
                    }
                    break;
                case ProtocolType.Speed:
                    if (mUdp != null)
                    {
                        return;
                    }
                    break;
                default:
                    Debug.Log("[ SOCKET ] unknow protocol");
                    break;
            }
            var result = acceptConn.IndexOf(protocolType);
            if (result >= 0) return;
            acceptConn.Add(protocolType);
            //Debug.Log("[ SOCKET ] send protocol:" + protocolType.Protocol());
            SendByte(MessageType.Protocol, protocolTmp, CompressType.None, protocolType.Protocol().GetBytes());
        }

        public void Ping()
        {
            //Debug.Log("[ SOCKET ] send ping");
            LastPingTimestamp = LocalTimestamp();
            //todo reconnect
            SendByte(MessageType.Ping, ProtocolType.Default, CompressType.None, null);
        }

        internal void Packet(NsData data, Namespace n)
        {
            if (n.messageQueue == MessageQueue.Off)
            {
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
            }
            else
            {
                var p = new QueueData();
                p.ns = n;
                p.data = data;
                outQueue.Enqueue(p);
                //Debug.Log(outQueue.Count);
            }
        }

        private ProtocolConn SelectConn(ProtocolType protocolType)
        {
            ProtocolConn conn;
            bool flag = false;
            bool result = allowConn.TryGetValue(protocolType, out conn);
            if (!result) flag = true;
            if (conn != null && !conn.Available)
            {
                Reconnect(protocolType);
                flag = true;
            }

            if (flag)
            {
                var enumerator = allowConn.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var _conn = enumerator.Current;
                    if (!_conn.Value.Available) continue;
                    conn = _conn.Value;
                    Protocol(_conn.Key, protocolType);
                }
            }
            return conn;
        }

        internal void Send(MessageType messageType, ProtocolType protocolType, CompressType compressType, DataPacket data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
            Write(messageType, conn, compressType, data.Packet);
        }

        internal void SendByte(MessageType messageType, ProtocolType protocolType, CompressType compressType, byte[] data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
            SendByte(messageType, protocolType, compressType, data, conn);
        }

        internal void SendByte(MessageType messageType, ProtocolType protocolType, CompressType compressType, byte[] data, ProtocolConn conn)
        {
            Write(messageType, conn, compressType, (writer) =>
            {
                if (data != null)
                    writer.Write(data, 0, data.Length);
            });
        }

        void dispatch(ProtocolConn conn, ProtocolType protocolType, Message message)
        {
            string v;
            switch (message.type)
            {
                case MessageType.Open:
                    v = new StreamReader(message.reader).ReadToEnd();
                    //Debug.Log("[ SOCKET ] accept open:" + v);
                    OnOpen(conn, protocolType, v);
                    break;
                case MessageType.Close:
                    //Debug.Log("[ SOCKET ] accept close");
                    OnClose();
                    break;
                case MessageType.Pong:
                    //ServerTimestamp = new StreamReader(message.reader).ReadToEnd();
                    //Debug.Log("[ SOCKET ] accept pong:"+protocolType);
                    OnPong(conn, protocolType);
                    break;
                case MessageType.Protocol:
                    v = new StreamReader(message.reader).ReadToEnd();
                    //Debug.Log("[ SOCKET ] accept Protocol:" + v);
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
            if (exitStatus != ExitStatus.Client)
                exitStatus = ExitStatus.Server;
            destroy("server is close");
        }
        private void OnOpen(ProtocolConn conn, ProtocolType protocolType, string sid)
        {
            conn.Available = true;
            // 确定连接后移除索引
            acceptConn.Remove(protocolType);

            ResetReconnect();
            this.sid = sid;
            if (state == ConnectionState.Connecting)
            {
                state = ConnectionState.Connected;
                Send(MessageType.Application, protocolType, CompressType.None, app);
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
                Send(MessageType.Application, protocolType, CompressType.None, app);
                listener.OnReconnect(true);
                Root().Reconnect();
            }
        }

        private void OnPong(ProtocolConn conn, ProtocolType protocolType)
        {
            if (LastPingTimestamp > 0)
            {
                PingTime = (LocalTimestamp() - LastPingTimestamp) / 2;
            }
        }

        private void OnProtocol(string url)
        {
            var u = new Uri(url);
            switch (u.Scheme.ToProtocol())
            {
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
            for (var i = 0; i < nsData.Length; i++)
            {
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
            Debug.Log(reason);
            var enumerator = allowConn.Values.GetEnumerator();
            var list = new List<ProtocolConn>(allowConn.Values);
            foreach (var conn in list)
            {
                if (conn != null) conn.Close();
            }
            allowConn.Clear();
            acceptConn.Clear();
            state = ConnectionState.Disconnected;
        }
    }
}

