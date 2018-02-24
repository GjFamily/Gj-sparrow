using UnityEngine;
using System.Collections;
using System.IO;

namespace Gj.Galaxy.Network
{
    public enum ProtocolType : byte
    {
        Default,
        Speed,
        Safe
    }

    public enum MessageQueue : byte
    {
        Off,
        On
    }

    public static class Extensions
    {
        public static string Protocol(this ProtocolType protocol)
        {
            switch(protocol){
                default:
                case ProtocolType.Default:
                    return "tcp";
                case ProtocolType.Safe:
                    return "websocket";
                case ProtocolType.Speed:
                    return "udp";
            }
        }
        public static ProtocolType ToProtocol(this string protocol)
        {
            switch (protocol)
            {
                default:
                case "tcp":
                    return ProtocolType.Default;
                case "websocket":
                    return ProtocolType.Safe;
                case "udp":
                    return ProtocolType.Speed;
            }
        }

        public static byte[] GetBytes(this string s){
            if (s == null) return null;
            return System.Text.Encoding.Default.GetBytes(s);
        }

        public static string GetString(this byte[] b){
            if (b == null) return null;
            return System.Text.Encoding.Default.GetString(b);
        }
    }
    public interface ProtocolConn{
        Stream Read(int head, out byte[] headB);
        bool Write(byte[] head, Stream reader);
        void Close();
    }


    public interface Protocol
    {
    }
}

