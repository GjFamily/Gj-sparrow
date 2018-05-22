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

	public abstract class Client
    {
        const int headLength = 1;
        const int maxLengthBytes = 5;

        public ClientListener listener;
		protected ConnectionState state = ConnectionState.Disconnected;
        public ConnectionState State
        {
            get
            {
                return state;
            }
        }
        protected string sid = "";
		protected string webSocketPath = "";
        protected AppPacket app;
        protected CompressType compressType = CompressType.Snappy;
        private ExitStatus exitStatus = ExitStatus._;

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

		protected abstract void OnConnConnect(ProtocolConn conn);
		protected abstract void OnAccept(ProtocolConn conn, ProtocolType protocolType, Message message);
        
		protected bool StartConnect()
		{
            if (app == null)
                throw new Exception("Please set app info");
            if (IsRuning) return true;
            sid = "";
            state = ConnectionState.Connecting;
            exitStatus = ExitStatus._;
			return false;
		}

		protected void CloseConnect()
        {
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
		public void Refresh()
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
					_conn.Refresh();
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

        internal bool Reconnect(ProtocolType protocolType)
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
			//Debug.Log(webSocketPath);
			Uri uri = new Uri(url + webSocketPath);
			Debug.Log(uri);
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
            var conn = new TcpSocket(point);
            return Accept(ProtocolType.Default, conn);
            //return true;
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
            Debug.Log("[ SOCKET ] accept conn" + protocolType);

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
					message.code = (byte)(h[0] >> 1);
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
                    OnAccept(conn, protocolType, message);               
                    message.Close();
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
                    //Debug.Log("success");
					OnConnConnect(conn);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }, () => {
                try
				{
					//Debug.Log("close");
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
        
        protected bool Write(byte code, ProtocolConn conn, CompressType compressType, Action<Stream> handler)
        {
            var compress = compressType == CompressType.None ? 0 : 1;
            header[0] = (byte)((byte)code << 1 & 0xff | (byte)compress & 1);
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

		protected void OnClose()
		{
			state = ConnectionState.Disconnected;
			if (exitStatus != ExitStatus.Client)
				exitStatus = ExitStatus.Server;
			listener.OnDisconnect();
			destroy("server is close");
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

