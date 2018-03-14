using System;
using System.Text;
using System.IO;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#else
using WebSocketSharp;
using System.Collections.Generic;
using System.Security.Authentication;
#endif


namespace Gj.Galaxy.Network
{
    public class WebSocketAgent:ProtocolConn
    {
        private Uri mUrl;

        public WebSocketAgent(Uri url)
        {
            mUrl = url;

            string protocol = mUrl.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);
        }

        ~WebSocketAgent(){
            Close();
        }

        public void SendString(string str)
        {
            Send(Encoding.UTF8.GetBytes (str));
        }

        public string RecvString()
        {
            byte[] retval = Recv();
            if (retval == null)
                return null;
            return Encoding.UTF8.GetString (retval);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int SocketCreate (string url);

        [DllImport("__Internal")]
        private static extern int SocketState (int socketInstance);

        [DllImport("__Internal")]
        private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
        private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

        [DllImport("__Internal")]
        private static extern int SocketRecvLength (int socketInstance);

        [DllImport("__Internal")]
        private static extern void SocketClose (int socketInstance);

        [DllImport("__Internal")]
        private static extern int SocketError (int socketInstance, byte[] ptr, int length);

        int m_NativeRef = 0;

        public void Send(byte[] buffer)
        {
            SocketSend (m_NativeRef, buffer, buffer.Length);
        }

        public byte[] Recv()
        {
            int length = SocketRecvLength (m_NativeRef);
            if (length == 0)
                return null;
            byte[] buffer = new byte[length];
            SocketRecv (m_NativeRef, buffer, length);
            return buffer;
        }

        public void Connect()
        {
            m_NativeRef = SocketCreate (mUrl.ToString());
        Debug.Log(m_NativeRef);
            //while (SocketState(m_NativeRef) == 0)
            //    yield return 0;
        }

        public void Close()
        {
            m_IsConnected = false;
            SocketClose(m_NativeRef);
        }

        public bool Connected()
        {
            return SocketState(m_NativeRef) != 0;
        }

        public string Error()
        {
            if(!m_IsConnected) return "";
            const int bufsize = 1024;
            byte[] buffer = new byte[bufsize];
            int result = SocketError (m_NativeRef, buffer, bufsize);

            if (result == 0)
                return null;

            return Encoding.UTF8.GetString (buffer);
        }
#else
        WebSocket m_Socket;
        Queue<byte[]> m_Messages = new Queue<byte[]>();
        bool m_IsConnected = false;
        string m_Error = null;

        public void Connect(Action open, Action close, Action message, Action<Exception> error)
        {
            m_Socket = new WebSocket(mUrl.ToString());// modified by TS
            m_Socket.EnableRedirection = true;
            //m_Socket.SslConfiguration.EnabledSslProtocols = m_Socket.SslConfiguration.EnabledSslProtocols | (SslProtocols)(3072| 768);
            m_Socket.OnMessage += (sender, e) =>
            {
                m_Messages.Enqueue(e.RawData);
                message();
            };
            m_Socket.OnOpen += (sender, e) =>
            {
                m_IsConnected = true;
                open();
            };
            m_Socket.OnError += (sender, e) =>
            {
                if (!m_IsConnected){
                    error(new Exception(e.Message));
                    m_Error = e.Message + (e.Exception == null ? "" : " / " + e.Exception);
                }
            };
            m_Socket.OnClose += (sender, e) =>
            {
                m_IsConnected = false;
                close();
            };
            m_Socket.Connect();
        }

        public bool Connected(){
            return m_IsConnected;
        }


        public void Send(byte[] buffer)
        {
            m_Socket.Send(buffer);
        }

        public byte[] Recv()
        {
            if (m_Messages.Count == 0)
                return null;
            return m_Messages.Dequeue();
        }

        public void Close()
        {
            m_IsConnected = false;
            if(m_Socket != null)
                m_Socket.Close();
        }

        public string Error()
        {
            var result =  m_Error;
            m_Error = null;
            return result;
        }
        #endif

        public Stream Read(int head, out byte[] headB)
        {
            var b = Recv();
            if (b == null){
                headB = null;
                return null;
            }
            var s = new MemoryStream(b, head, b.Length - head, false);
            headB = new byte[head];
            for (var i = 0; i < head; i++){
                headB[i] = b[i];
            }
            return s;
        }

        public bool Write(byte[] head, Stream reader)
        {
            if (!Connected()) return false;
            int rl = Convert.ToInt32(reader.Length);
            int length = head.Length + rl;
            byte[] sum = new byte[length];
            head.CopyTo(sum, 0);
            //Debug.Log(new StreamReader(reader).ReadToEnd());
            int result = reader.Read(sum, head.Length, rl);
            //Debug.Log(length);
            //Debug.Log(head[0]);
            //Debug.Log(result);
            //if(rl > 0){
            //    Debug.Log(sum[9]);
            //    Debug.Log(sum[10]);
            //    Debug.Log(sum[11]);
            //}
            Send(sum);
            return true;
        }
    }
}
