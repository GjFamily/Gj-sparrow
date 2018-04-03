using UnityEngine;
using System.Collections;
using System.IO;
using System;

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
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        public static string GetString(this byte[] b){
            if (b == null) return null;
            return System.Text.Encoding.UTF8.GetString(b);
        }
    }
    public interface ProtocolConn{
        void Connect(Action open, Action close, Action message, Action<Exception> error);
        Stream Read(int head, out byte[] headB);
        bool Write(byte[] head, Stream reader);
        bool Connected();
        bool Connecting();
        void Accept();
        void Close();
    }

    public class SwitchQueue<T> where T : class
    {

        private Queue mConsumeQueue;
        private Queue mProduceQueue;

        public SwitchQueue()
        {
            mConsumeQueue = new Queue(16);
            mProduceQueue = new Queue(16);
        }

        public SwitchQueue(int capcity)
        {
            mConsumeQueue = new Queue(capcity);
            mProduceQueue = new Queue(capcity);
        }

        // producer
        public void Push(T obj)
        {
            lock (mProduceQueue)
            {
                mProduceQueue.Enqueue(obj);
            }
        }

        // consumer.
        public T Pop()
        {

            return (T)mConsumeQueue.Dequeue();
        }

        public bool Empty()
        {
            return 0 == mConsumeQueue.Count;
        }

        public void Switch()
        {
            lock (mProduceQueue)
            {
                Utility.Swap(ref mConsumeQueue, ref mProduceQueue);
            }
        }

        public void Clear()
        {
            lock (mProduceQueue)
            {
                mConsumeQueue.Clear();
                mProduceQueue.Clear();
            }
        }
    }
}

