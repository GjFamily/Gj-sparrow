using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Gj.Galaxy.Network
{
    public class TcpSocket : ProtocolConn
    {
        private IPEndPoint mSvrEndPoint;
        private TcpClient mTcpClient;
        private TcpStateObject state;

        private bool available = false;
        public bool Available
        {
            get
            {
                return available;
            }

            set
            {
                available = value;
            }
        }

        public bool Connecting
        {
            get
            {
                return state != null && state.open != null;
            }
        }

        public bool Connected
        {
            get
            {
                return mTcpClient != null && mTcpClient.Connected;
            }
        }

        public TcpSocket(IPEndPoint point)
        {
            mSvrEndPoint = point;
            mTcpClient = new TcpClient();
            state = new TcpStateObject(mTcpClient);
        }

        ~TcpSocket()
        {
            Close();
        }
        public void Connect(Action open, Action close, Action<Exception> error)
        {
            state.open = open;
            state.close = close;
            state.error = error;
            state.Start(mSvrEndPoint);
        }

        public Stream Read(int head, out byte[] headB)
        {
            headB = new byte[head];
            Stream stream = state.GetStream();
            stream.Read(headB, 0, head);
            return stream;
        }

        public bool Write(byte[] head, Stream reader)
        {
            if (!Connected) return false;
            int rl = Convert.ToInt32(reader.Length);
            byte[] sum = new byte[rl];
            int result = reader.Read(sum, 0, rl);
            return state.Send(head, sum);
        }

        public void Close()
        {
            available = false;
            state.Close();
            state = null;
        }

        public void Update()
        {
            // pass, Tcp stream
        }

        public void Accept(ref byte[] head, Action callback)
        {
            state.tmp_head = head;
            state.message = callback;
        }

        public void Read(ref byte[] content, Action callback)
        {
            state.Read(ref content, callback);
        }
    }

    internal class TcpStateObject
    {
        public TcpClient tcpClient;
        public Action open;
        public Action close;
        public Action<Exception> error;

        public Action message;
        public Action ReadCallback;
        public byte[] tmp_head;
        public byte[] content;

        public TcpStateObject(TcpClient client)
        {
            tcpClient = client;
        }

        public Stream GetStream()
        {
            return tcpClient.GetStream();
        }

        public void Start(IPEndPoint point)
        {
            
            tcpClient.BeginConnect(point.Address, point.Port, new AsyncCallback(ConnectCallback), this);
        }

        public bool Send(byte[] head, byte[] body)
        {
            NetworkStream stream = tcpClient.GetStream();
            if (stream.CanWrite)
            {
                //stream.Write(new byte[] { 0x08, 0x21 }, 0, 2);
                stream.Write(head, 0, head.Length);
                stream.Write(body, 0, body.Length);
                return true;
            }
            else
            {
                throw new IOException();
            }
        }

        public void Read(ref byte[] content, Action callback){
            NetworkStream stream = tcpClient.GetStream();
            if(content.Length > 0){
                ReadCallback = callback;
                this.content = content;

                stream.BeginRead(content, 0, content.Length, new AsyncCallback(EndReadCallback), this);
            }else{
                callback();

                stream.BeginRead(tmp_head, 0, tmp_head.Length, new AsyncCallback(EndAcceptCallback), this);
            }
        }

        public void EndAcceptCallback(IAsyncResult ar)
        {
            NetworkStream stream = tcpClient.GetStream();

            int bytesRead = stream.EndRead(ar);
            if(bytesRead < tmp_head.Length){
                error(new IOException("Tcp Accept Head error"));
            }else{
                message();
            }
        }

        public void EndReadCallback(IAsyncResult ar)
        {
            NetworkStream stream = tcpClient.GetStream();

            int bytesRead = stream.EndRead(ar);

            if(bytesRead < content.Length){
                error(new IOException("Tcp Accept Content error"));
            }else{
                ReadCallback();
            }
            stream.BeginRead(tmp_head, 0, tmp_head.Length, new AsyncCallback(EndAcceptCallback), this);
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                if (tcpClient.Client != null)
                {
                    tcpClient.EndConnect(ar);
                    if(tcpClient.Connected){
                        open();
                        var stream = tcpClient.GetStream();
                        stream.BeginRead(tmp_head, 0, tmp_head.Length, new AsyncCallback(EndAcceptCallback), this);
                    }else{
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                error(ex);
                Close();
            }
        }

        public void Close()
        {
            if (tcpClient.Connected)
            {
                tcpClient.Close();
            }
            close();
        }
    }
}