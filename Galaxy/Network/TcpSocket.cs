using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Gj.Galaxy.Network
{
    public class TcpSocket: ProtocolConn
    {
        private IPEndPoint mSvrEndPoint;
        private TcpClient mTcpClient;
        private TcpStateObject state;

        public TcpSocket(IPEndPoint point)
        {
            mSvrEndPoint = point;
            mTcpClient = new TcpClient();
        }

        ~TcpSocket()
        {
            Close();
        }
        public void Connect(Action open, Action close, Action message, Action<Exception> error){
            state = new TcpStateObject(mTcpClient);
            state.open = open;
            state.close = close;
            state.message = message;
            state.error = error;
            state.Start(mSvrEndPoint);
        }

        public Stream Read(int head, out byte[] headB){
            headB = new byte[head];
            Stream stream = state.GetStream();
            stream.Read(headB, 0, head);
            return stream;
        }

        public bool Write(byte[] head, Stream reader){
            if (!Connected()) return false;
            int rl = Convert.ToInt32(reader.Length);
            byte[] sum = new byte[rl];
            int result = reader.Read(sum, 0, rl);
            return state.Send(head, sum);
        }
        public bool Connected(){
            //return socket ? true : false;
            return mTcpClient.Client != null;
        }

        public bool Connecting(){
            return state != null;
        }

        public void Close()
        {
            if (Connected())
            {
                mTcpClient.Close();
            }
            state.close();
            state = null;
        }

        public void Accept(){
            // pass, Tcp stream
        }
    }

    internal class TcpStateObject
    {
        public TcpClient tcpClient;
        public Action open;
        public Action close;
        public Action message;
        public Action<Exception> error;
        public readonly byte[] Buffer;
        public readonly int BufferSize = 2;

        public TcpStateObject(TcpClient client)
        {
            tcpClient = client;
            this.Buffer = new byte[this.BufferSize];
        }

        public Stream GetStream(){
            return tcpClient.GetStream();
        }

        public void Start(IPEndPoint point)
        {
            tcpClient.BeginConnect(point.Address, point.Port, new AsyncCallback(ConnectCallback), this);
        }

        public bool Send(byte[] head, byte[] body){
            NetworkStream stream = tcpClient.GetStream();
            if (stream.CanWrite)
            {
                stream.Write(new byte[] { 0x08, 0x21 }, 0, 2);
                stream.Write(head, 0, head.Length);
                stream.Write(body, 0, body.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void EndReadCallback(IAsyncResult ar)
        {
            NetworkStream stream = tcpClient.GetStream();

            int bytesRead = stream.EndRead(ar);

            if (bytesRead == BufferSize && Buffer[0] == 0x08 && Buffer[1] == 0x21)
            {
                message();
            }
            stream.BeginRead(Buffer, 0, BufferSize, new AsyncCallback(EndReadCallback), this);
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                if (tcpClient.Client != null)
                {
                    tcpClient.EndConnect(ar);
                    open();
                    var stream = tcpClient.GetStream();
                    stream.BeginRead(Buffer, 0, BufferSize, new AsyncCallback(EndReadCallback), this);
                }
            }
            catch (Exception ex)
            {
                tcpClient.Close();
                close();
            }
        }
    }
}