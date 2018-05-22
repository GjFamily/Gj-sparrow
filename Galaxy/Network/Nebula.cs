using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Gj.Galaxy.Network
{
    internal enum NebulaType : byte
    {
        Open = 0,
        Reopen,
        Close,
        Ping,
		Pong,
        Application,
        Namespace,
        Protocol,
    }
	public class Nebula : Client
	{
		protected Namespace root;

		protected ProtocolConn conn;

        private Queue<QueueData> outQueue = new Queue<QueueData>();
        private Queue<QueueData> inQueue = new Queue<QueueData>();

        public Nebula()
        {
            root = new Namespace(this);
			webSocketPath = "/nebula.socket";
		}


        public bool Connect(string url)
        {
            Debug.Log("[ SOCKET ] connect");
			if(StartConnect())
			{
				return true;
			}
            var result = WebSocket(url);
            return result;
        }

        public Namespace Of(byte ns)
        {
            return root.Of(ns);
        }

        public Namespace Root()
        {
            return root;
        }

        public void Close()
		{
			Disconnect();
		}

		public void Disconnect()
        {
            Debug.Log("[ SOCKET ] disconnect");
			if (IsConnected)
            {
                SendByte(NebulaType.Close, false, null);
            }
			CloseConnect();
        }

		protected override void OnConnConnect(ProtocolConn conn)
		{
			//Debug.Log(sid.GetBytes().GetString());
			this.conn = conn;
            if (sid == "")
            {
                Debug.Log("[ SOCKET ] send open");
				SendByte(NebulaType.Open, false, null);
            }
            else
            {
                Debug.Log("[ SOCKET ] send reopen:" + sid);
				SendByte(NebulaType.Reopen, false, sid.GetBytes());
            }
		}

		protected override void OnAccept(ProtocolConn conn, ProtocolType protocolType, Message message)
        {
            string v;
			NebulaType type = (NebulaType)(message.code >> 3);
			switch (type)
            {
				case NebulaType.Open:               
                    Debug.Log("[ SOCKET ] accept open");
					v = message.ReadString();
					OnOpen(conn, protocolType, v);
                    break;
				case NebulaType.Reopen:
                    Debug.Log("[ SOCKET ] accept reopen");
                    OnReopen(conn, protocolType);
                    break;
				case NebulaType.Close:
                    Debug.Log("[ SOCKET ] accept close");
                    OnClose();
                    break;
				case NebulaType.Pong:
                    //ServerTimestamp = new StreamReader(message.reader).ReadToEnd();
                    Debug.Log("[ SOCKET ] accept pong");
                    OnPong(conn, protocolType);
                    break;
				case NebulaType.Namespace:
					OnNamespace(message.Reader());
					break;
                //case NebulaType.Protocol:
                //message.GetReader((reader) =>
                //{
                //    v = new StreamReader(message.reader).ReadToEnd();
                //    //Debug.Log("[ SOCKET ] accept Protocol:" + v);
                //    OnProtocol(v);
                //});
                //break;
                default:
                    Debug.Log(string.Format("Client accept error type:{0}", message.code));
                    break;
            }
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
					Send(NebulaType.Namespace, compress, a);
                    a = new NsDataArray();
                }
                l.Add(data.data);
            } while (t <= times);
            if (l.Count > 0)
            {
                a.data = l.ToArray();
                l.Clear();
				Send(NebulaType.Namespace, compress, a);
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


        public void Ping()
        {
            Debug.Log("[ SOCKET ] send ping");
            LastPingTimestamp = LocalTimestamp();
            // todo reconnect
			SendByte(NebulaType.Ping, false, null);
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
				Send(NebulaType.Application, false, app);
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
				Send(NebulaType.Application, false, app);
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
				Send(NebulaType.Application, false, app);
                listener.OnConnect(true);
            }
            else if (state == ConnectionState.Reconnecting)
            {
                state = ConnectionState.Connected;
				Send(NebulaType.Application, false, app);
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
        
        private void OnNamespace(Stream reader)
        {
			var nsData = NsDataArray.Unpack(reader);
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
        
		internal void Send(NebulaType type, bool compress, DataPacket data)
        {
			Write((byte)((byte)type << 3 & 0xff), conn, compress ? compressType : CompressType.None, data.Packet);
        }
        
		internal void SendByte(NebulaType type, bool compress, byte[] data)
        {
			Write((byte)((byte)type << 3 & 0xff), conn, compress ? compressType : CompressType.None, (writer) =>
            {
                if (data != null)
                    writer.Write(data, 0, data.Length);
            });
        }
  
	}
}