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
        private Action message;
        private SwitchQueue<byte[]> m_Messages = new SwitchQueue<byte[]>(128);
        private byte[] current_data;
        private byte[] tmp_head;
        private int position = 0;

        WebSocket m_Socket;
        bool m_IsConnected = false;
        string m_Error = null;

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
                return m_Socket != null;
            }
        }

        public bool Connected
        {
            get
            {
                //return socket ? true : false;
                return m_IsConnected;
            }
        }
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


        public void Connect(Action open, Action close, Action<Exception> error)
        {
            m_Socket = new WebSocket(mUrl.ToString());// modified by TS
            m_Socket.EnableRedirection = true;
            //m_Socket.SslConfiguration.EnabledSslProtocols = m_Socket.SslConfiguration.EnabledSslProtocols | (SslProtocols)(3072| 768);
            m_Socket.OnMessage += (sender, e) =>
            {
                m_Messages.Push(e.RawData);
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

        public void Send(byte[] buffer)
        {
            m_Socket.Send(buffer);
        }

        public byte[] Recv()
        {
            return m_Messages.Pop();
        }

        public void Close()
        {
            available = false;
            m_IsConnected = false;
            if(m_Socket != null)
                m_Socket.Close();
            m_Socket = null;
        }

        public string Error()
        {
            var result =  m_Error;
            m_Error = null;
            return result;
        }

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
            if (!Connected) return false;
            int rl = Convert.ToInt32(reader.Length);
            int length = head.Length + rl;
            byte[] sum = new byte[length];
            head.CopyTo(sum, 0);
            int result = reader.Read(sum, head.Length, rl);
            Send(sum);
            return true;
        }

        public void Update()
        {
            m_Messages.Switch();
            while(!m_Messages.Empty())
            {
                current_data = Recv();
                Array.Copy(current_data, tmp_head, tmp_head.Length);
                position += tmp_head.Length;
                message();
            }
        }

        public void Accept(ref byte[] head, Action callback)
        {
            tmp_head = head;
            message = callback;
        }

        public void Read(ref byte[] content, Action callback)
        {
            if(content.Length > 0){
                Array.Copy(current_data, position, content, 0, content.Length);
                position += content.Length;
            }
            callback();
        }

        public void Read(ref byte[] content)
        {
            if (content.Length > 0)
            {
                Array.Copy(current_data, position, content, 0, content.Length);
                position += content.Length;
            }
        }

        public void Release()
        {
            position = 0;
        }
    }
}
