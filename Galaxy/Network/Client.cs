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
        const int headLength = 1;
        const int maxLengthBytes = 5;

        public ClientListener listener;
        private ConnectionState state = ConnectionState.Disconnected;
        public ConnectionState State
        {
            get
            {
                return state;
            }
        }
        protected string sid = "";
        private AppPacket app;
        protected Namespace root;
        protected CompressType compressType = CompressType.Snappy;
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
        protected static Func<Stream, Stream> readerHandle;
        protected static Func<Stream, Stream> writerHandle;
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

        private byte[] header = new byte[maxLengthBytes];

        public LogLevel logLevel = LogLevel.Error;

        public Client()
        {
            root = new Namespace(this);
            if(compressType == CompressType.Snappy){
                readerHandle = (Stream arg1) =>
                {
                    return new SnappyStream(arg1, CompressionMode.Decompress);
                };
                writerHandle = (Stream arg1) =>
                {
                    return new SnappyStream(arg1, CompressionMode.Compress);
                };
            }
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
            sid = "";
            state = ConnectionState.Connecting;
            exitStatus = ExitStatus._;
            var result = WebSocket(url);
            return result;
        }

        public void Disconnect()
        {
            Debug.Log("[ SOCKET ] disconnect");
            if (IsRuning)
            {
                SendByte(MessageType.Close, ProtocolType.Default, false, null);
            }
            state = ConnectionState.Disconnecting;
            exitStatus = ExitStatus.Client;
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
            //var conn = new UdpSocket(point);
            //return Accept(ProtocolType.Speed, conn);
            return true;
        }

        protected bool Accept(ProtocolType protocolType, ProtocolConn conn)
        {
            if (allowConn.ContainsKey(protocolType)) allowConn.Remove(protocolType);
            allowConn.Add(protocolType, conn);
            //Debug.Log("[ SOCKET ] accept conn" + protocolType);

            var h = new byte[headLength];
            var lengthByte = new byte[1];
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
                    var compress = (h[0] & 1);
                    //message.time = BitConverter.ToInt32(h, 1);

                    message.conn = conn;
                    message.GetReader = (action) =>
                    {
                        var i = headLength;
                        while (i < maxLengthBytes)
                        {
                            conn.Read(ref lengthByte);
                            length += (int)(lengthByte[0] & 127) * multiplier;
                            multiplier *= 128;
                            if ((lengthByte[0] & 128) == 0)
                            {
                                break;
                            }
                        }
                        //Debug.Log(length);
                        var b = new byte[length];
                        conn.Read(ref b, () =>
                        {
                            var buffer = new MemoryStream(b, false);
                            //Debug.Log("compress:" + compressType);
                            if (compress > 0)
                            {
                                message.reader = readerHandle(buffer);
                            }
                            else
                            {
                                message.reader = buffer;
                            }

                            //ServerTimestamp = message.time;
                            if (InData != null) InData(headLength + length);

                            action(message.reader);
                        });
                    };
                    dispatch(conn, protocolType, message);
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
                    //Debug.Log(sid.GetBytes().GetString());
                    if (sid == ""){
                        Debug.Log("[ SOCKET ] send open");
                        SendByte(MessageType.Open, conn, false, null);
                    }else{
                        Debug.Log("[ SOCKET ] send reopen:" + sid);
                        SendByte(MessageType.Reopen, conn, false, sid.GetBytes());
                    }
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
            bool compress = false;
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
            var compress = compressType == CompressType.None ? 0 : 1;
            header[0] = (byte)((byte)messageType << 4 & 0xff | (byte)compress & 1);

            Stream buffer = new MemoryStream();
            if (compressType != CompressType.None)
            {
                var w = writerHandle(buffer);
                handler(w);
                w.Flush();
            }
            else
            {
                handler(buffer);
            }
            buffer.Position = 0;
            var length = buffer.Length;

            var i = headLength;
            while(length > 0){
                if (i > maxLengthBytes)
                {
                    Debug.LogError("max length fail");
                    break;
                }
                var b = length % 128;
                length = length / 128;

                if (length > 0)
                {
                    b = b | 128;
                }
                header[i] = (byte)(b & 0xff);
                i++;
            }
            var realHeader = new byte[i];
            LastTimestamp = LocalTimestamp();
            if (OutData != null) OutData(i + buffer.Length);

            Array.Copy(header, realHeader, i);
            return conn.Write(realHeader, buffer);
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
            SendByte(MessageType.Protocol, protocolTmp, false, protocolType.Protocol().GetBytes());
        }

        public void Ping()
        {
            //Debug.Log("[ SOCKET ] send ping");
            LastPingTimestamp = LocalTimestamp();
            //todo reconnect
            SendByte(MessageType.Ping, ProtocolType.Default, false, null);
        }

        internal void Packet(NsData data, Namespace n)
        {
            //if (n.messageQueue == MessageQueue.Off)
            //{
            //    bool compress = false;
            //    ProtocolType protocol = ProtocolType.Default;
            //    if (n != null)
            //    {
            //        compress = n.compress;
            //        protocol = n.protocol;
            //    }
            //    var a = new NsDataArray();
            //    a.data = new NsData[] { data };
            //    Send(MessageType.Namespace, protocol, compress, a);
            //}
            //else
            //{
                var p = new QueueData();
                p.ns = n;
                p.data = data;
                outQueue.Enqueue(p);
                //Debug.Log(outQueue.Count);
            //}
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

        internal void Send(MessageType messageType, ProtocolType protocolType, bool compress, DataPacket data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
            Write(messageType, conn, compress ? compressType : CompressType.None, data.Packet);
        }

        internal void SendByte(MessageType messageType, ProtocolType protocolType, bool compress, byte[] data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
            SendByte(messageType, conn, compress, data);
        }

        internal void SendByte(MessageType messageType, ProtocolConn conn, bool compress, byte[] data)
        {
            Write(messageType, conn, compress ? compressType : CompressType.None, (writer) =>
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
                    message.GetReader((reader) =>
                    {
                        v = new StreamReader(reader).ReadToEnd();
                        //Debug.Log("[ SOCKET ] accept open:" + v);
                        OnOpen(conn, protocolType, v);
                    });
                    break;
                case MessageType.Reopen:
                    OnReopen(conn, protocolType);
                    break;
                case MessageType.Close:
                    //Debug.Log("[ SOCKET ] accept close");
                    OnClose();
                    break;
                case MessageType.Pong:
                    //ServerTimestamp = new StreamReader(message.reader).ReadToEnd();
                    //Debug.Log("[ SOCKET ] accept pong:"+protocolType);
                    message.GetReader((reader) =>
                    {
                        v = new StreamReader(reader).ReadToEnd();
                        //Debug.Log("[ SOCKET ] accept open:" + v);
                        OnPong(conn, protocolType);
                    });
                    break;
                case MessageType.Protocol:
                    message.GetReader((reader) =>
                    {
                        v = new StreamReader(message.reader).ReadToEnd();
                        //Debug.Log("[ SOCKET ] accept Protocol:" + v);
                        OnProtocol(v);
                    });
                    break;
                case MessageType.Namespace:
                    message.GetReader((reader) =>
                    {
                        OnNamespace(message);
                    });

                    break;
                default:
                    Debug.Log(string.Format("Client accept error type:{0}", message.type));
                    break;
            }
            message.Close();
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
                Send(MessageType.Application, protocolType, false, app);
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
                Send(MessageType.Application, protocolType, false, app);
                listener.OnReconnect(true);
                Root().Reconnect();
            }
        }

        private void OnReopen(ProtocolConn conn, ProtocolType protocolType)
        {
            conn.Available = true;
            // 确定连接后移除索引
            acceptConn.Remove(protocolType);

            ResetReconnect();
            if (state == ConnectionState.Connecting)
            {
                state = ConnectionState.Connected;
                Send(MessageType.Application, protocolType, false, app);
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
                Send(MessageType.Application, protocolType, false, app);
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
                //if (n.messageQueue == MessageQueue.Off)
                //{
                //    n.dispatch(data);
                //}
                //else
                //{
                    var p = new QueueData();
                    p.ns = n;
                    p.data = data;
                    inQueue.Enqueue(p);
                //}
            }

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
            sid = "";
            allowConn.Clear();
            acceptConn.Clear();
            state = ConnectionState.Disconnected;
        }
    }
}

