using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

namespace Gj.Galaxy.Network
{
    internal enum CometType : byte
    {
        Open = 0,
        Reopen,
        Close,
        Ping,
        Pong,
        Application,
        Token,
        Protocol,
        Error,
    }
	public interface ServiceListener:ClientListener
    {
		void OnAccept(byte de, Message message);
		void OnAcceptQueue(byte de, object[] param);
		void Update();
    }

    public class CometProxy
	{
		internal byte no = 0;
		internal bool join = false;
        private ConnectionState state = ConnectionState.Disconnected;
        
		private static Queue<byte> inQueue = new Queue<byte>();
		private static Queue<object[]> inDataQueue = new Queue<object[]>();
		private static Queue<byte> outQueue = new Queue<byte>();
        private static Queue<byte[]> outDataQueue = new Queue<byte[]>();
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
		internal Comet comet;
		internal ServiceListener listener;
		internal bool compress = false;
		internal ProtocolType ProtocolType = ProtocolType.Default;

		internal CometProxy(byte n, Comet comet, ServiceListener listener)
		{
			this.no = n;
			this.comet = comet;
			this.listener = listener;
		}

        public void Join()
		{
			join = true;
			if (comet.IsConnected)
			{
				listener.OnConnect(true);
			}
			else
			{
				comet.Connect();
			}
		}

        public void Leave()
		{
			join = false;
			comet.Disconnect(false);
		}

        public void SetCompress(bool compress)
		{
			this.compress = compress;
		}

		public void SetProtocolType(ProtocolType protocolType)
		{
			this.ProtocolType = protocolType;
		}

		public void AcceptQueue(byte de, object[] data)
		{
			inQueue.Enqueue(de);
			inDataQueue.Enqueue(data);
		}

        public void Send(byte de, byte[] data)
		{
			outQueue.Enqueue(de);
			outDataQueue.Enqueue(data);
		}

        public void SendObject(byte de, object[] data)
		{
			var bytes = MessagePack.MessagePackSerializer.Serialize(data);
            outQueue.Enqueue(de);
			outDataQueue.Enqueue(bytes);
		}

		internal void InQueue()
		{
			var length = inQueue.Count;
			for (var i = 0; i < length;i++)
			{
				listener.OnAcceptQueue(inQueue.Dequeue(), inDataQueue.Dequeue());
			}
		}

        internal void OutQueue()
		{
			var length = outQueue.Count;
            for (var i = 0; i < length; i++)
			{
				this.comet.SendByte(no, outQueue.Dequeue(), ProtocolType, compress, outDataQueue.Dequeue());
            }
		}

	}
	public class Comet : Client, ClientListener
    {
		protected string token;
		protected string url;
		protected Dictionary<byte, CometProxy> services = new Dictionary<byte, CometProxy>();
		
		public Comet(string token)
		{
			listener = this;
			this.token = token;
			webSocketPath = "/comet.socket";
		}

        public void SetToken(string token)
		{
			this.token = token;
		}
        
		public void Connect()
        {
            Debug.Log("[ SOCKET ] connect");
            if (StartConnect())
            {
				return ;
            }
            WebSocket(url);
        }

		public void SwitchConnect(string url)
		{
			Disconnect(true);
			WebSocket(url);
		}

		public CometProxy Register(byte no, ServiceListener service)
		{
			services[no] = new CometProxy(no, this, service);
			return services[no];
		}

        public void Update()
		{
			if (!IsRuning) return;
			var a = services.Values.GetEnumerator();
			while(a.MoveNext())
			{
				var v = a.Current;
				v.listener.Update();
			}
		}

        public void OnConnect(bool success)
        {
			var a = services.Values.GetEnumerator();
            while (a.MoveNext())
            {
                var v = a.Current;
				if (v.join)
				    v.listener.OnConnect(success);
            }
        }

        public void OnReconnect(bool success)
        {
			var a = services.Values.GetEnumerator();
            while (a.MoveNext())
            {
                var v = a.Current;
                if (v.join)
					v.listener.OnReconnect(success);
            }
        }

        public void OnDisconnect()
        {
            var a = services.Values.GetEnumerator();
            while (a.MoveNext())
            {
				var v = a.Current;
                if (v.join)
					v.listener.OnDisconnect();
            }
        }

        public void Close()
        {
            Disconnect(true);
        }

		public void Disconnect(bool force)
		{
            Debug.Log("[ SOCKET ] disconnect");
            if (!force)
			{
				var a = services.Values.GetEnumerator();
                while (a.MoveNext())
                {
                    var v = a.Current;
					if (v.join)
						return;
                }
				
			}
			if (IsConnected)
            {
				SendByte(CometType.Close, ProtocolType.Default, false, null);
            }
            CloseConnect();
			OnDisconnect();
		}

		protected override void OnConnConnect(ProtocolConn conn)
		{
			//Debug.Log(sid.GetBytes().GetString());
            if (sid == "")
            {
                Debug.Log("[ SOCKET ] send open");
				SendByte(CometType.Open, conn, false, null);
            }
            else
            {
                Debug.Log("[ SOCKET ] send reopen:" + sid);
				SendByte(CometType.Reopen, conn, false, sid.GetBytes());
            }
		}

		protected override void OnAccept(ProtocolConn conn, ProtocolType protocolType, Message message)
		{
			string v;
			var t = (byte)(message.code & 0xf);
            var n = (byte)(message.code >> 3);
			if (n == 0) {
				var type = (CometType)t;
				switch (type)
                {
					case CometType.Open:
						v = message.ReadString();
						OnOpen(conn, protocolType, v);
                        break;
					case CometType.Reopen:
                        OnReopen(conn, protocolType);
                        break;
					case CometType.Close:
                        //Debug.Log("[ SOCKET ] accept close");
                        OnClose();
                        break;
					case CometType.Pong:
                        //ServerTimestamp = new StreamReader(message.reader).ReadToEnd();
                        //Debug.Log("[ SOCKET ] accept pong:"+protocolType);
                        OnPong(conn, protocolType);
                        break;
					case CometType.Protocol:
						v = message.ReadString();
						OnProtocol(v);
                        break;
                    default:
                        Debug.Log(string.Format("Client accept error type:{0}", message.code));
                        break;
                }
			} else {
				var service = services[n];
			}
		}
		public void Ping()
        {
			if (!IsConnected) return;
            //Debug.Log("[ SOCKET ] send ping");
            LastPingTimestamp = LocalTimestamp();
            //todo reconnect
			SendByte(CometType.Ping, ProtocolType.Default, false, null);
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
				Send(CometType.Application, conn, false, app);
                SendByte(CometType.Token, conn, false, token.GetBytes());
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
				Send(CometType.Application, conn, false, app);
                SendByte(CometType.Token, conn, false, token.GetBytes());
                listener.OnReconnect(true);
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
				Send(CometType.Application, conn, false, app);
				SendByte(CometType.Token, conn, false, token.GetBytes());
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
				Send(CometType.Application, conn, false, app);
				SendByte(CometType.Token, conn, false, token.GetBytes());
                listener.OnReconnect(true);
            }
        }

        private void OnPong(ProtocolConn conn, ProtocolType protocolType)
        {
            if (LastPingTimestamp > 0)
            {
                PingTime = (LocalTimestamp() - LastPingTimestamp) / 2;
            }
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
            SendByte(CometType.Protocol, protocolTmp, false, protocolType.Protocol().GetBytes());
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

		internal void Send(byte no, byte en, ProtocolType protocolType, bool compress, DataPacket data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
			Write((byte)((no << 4 & 0xff) | en), conn, compress ? compressType : CompressType.None, data.Packet);
        }

		internal void SendByte(byte no, byte en, ProtocolType protocolType, bool compress, byte[] data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
			Write((byte)((no << 4 & 0xff) | en), conn, compress ? compressType : CompressType.None, (writer) =>
            {
                if (data != null)
                    writer.Write(data, 0, data.Length);
            });
		}

		internal void Send(CometType type, ProtocolType protocolType, bool compress, DataPacket data)
        {
            var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
            //Debug.Log(conn);
			Send(type, conn, compress, data);
        }

		internal void Send(CometType type, ProtocolConn conn, bool compress, DataPacket data)
        {
            Write((byte)type, conn, compress ? compressType : CompressType.None, data.Packet);
        }

		internal void SendByte(CometType type, ProtocolType protocolType, bool compress, byte[] data)
		{
			var conn = SelectConn(protocolType);
            if (conn == null)
            {
                return;
            }
			SendByte(type, conn, compress, data);
        }

		internal void SendByte(CometType type, ProtocolConn conn, bool compress, byte[] data)
        {
			Write((byte)type, conn, compress ? compressType : CompressType.None, (writer) =>
            {
                if (data != null)
                    writer.Write(data, 0, data.Length);
            });
        }

        public void WriteQueue()
        {
			if (!IsConnected) return;
			var a = services.Values.GetEnumerator();
            while (a.MoveNext())
            {
				a.Current.OutQueue();
            }
        }

        public void ReadQueue()
		{
            if (!IsConnected) return;
			var a = services.Values.GetEnumerator();
            while (a.MoveNext())
            {
				a.Current.InQueue();
            }
        }
  
	}
}