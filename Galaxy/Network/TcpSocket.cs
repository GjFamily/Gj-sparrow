using System;
using System.Collections;
using System.IO;
using System.Net;

namespace Gj.Galaxy.Network
{
    public class TcpSocket: ProtocolConn
    {
        private int socket;
        public TcpSocket(IPEndPoint point)
        {
            //mUrl = url;

            //if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                //throw new ArgumentException("Unsupported protocol: " + protocol);
        }

        ~TcpSocket()
        {
            Close();
        }
        public void Connect(Action open, Action close, Action message, Action<Exception> error){
            
        }
        public Stream Read(int head, out byte[] headB){
            //var b = Recv();
            var b = new byte[10];
            if (b == null)
            {
                headB = null;
                return null;
            }
            var s = new MemoryStream(b, head, b.Length - head, false);
            headB = new byte[head];
            for (var i = 0; i < head; i++)
            {
                headB[i] = b[i];
            }
            return s;
        }
        public bool Write(byte[] head, Stream reader){
            if (!Connected()) return false;
            int rl = Convert.ToInt32(reader.Length);
            int length = head.Length + rl;
            byte[] sum = new byte[length];
            head.CopyTo(sum, 0);
            int result = reader.Read(sum, head.Length, rl);
            //Send(sum);
            return true;
        }
        public bool Connected(){
            //return socket ? true : false;
            return true;
        }
        public void Close()
        {
        }
    }
}