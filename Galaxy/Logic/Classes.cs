using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Network;
using System.IO;

namespace Gj.Galaxy.Logic{
    public interface GameObservable
    {
        void OnSerialize(StreamBuffer stream, MessageInfo info);
        void OnDeserialize(StreamBuffer stream, MessageInfo info);
        void Bind(NetworkEsse esse);
    }

    public interface GameSync
    {
        void SendSyncData(StreamBuffer stream);
        void ReceiveSyncData(StreamBuffer stream);
    }


    public struct MessageInfo
    {
        private readonly long timeInt;
        /// <summary>The sender of a message / event. May be null.</summary>
        public readonly GamePlayer sender;
        public readonly NetworkEsse esse;

        public MessageInfo(GamePlayer player, NetworkEsse esse)
        {
            this.sender = player;
            this.timeInt = PeerClient.LocalTimestamp;
            this.esse = esse;
        }

        public double timestamp
        {
            get
            {
                uint u = (uint)this.timeInt;
                double t = u;
                return t / 1000;
            }
        }

        public override string ToString()
        {
            return string.Format("[MessageInfo: Sender='{1}' Senttime={0}]", this.timestamp, this.sender);
        }
    }

    public class StreamBuffer
    {
        bool write = false;
        private Queue<object> writeData;
        private object[] readData;
        internal MemoryStream stream = new MemoryStream();
        internal byte currentItem = 0; //Used to track the next item to receive

        public StreamBuffer(bool write, object[] incomingData)
        {
            this.write = write;
            if (incomingData == null)
            {
                this.writeData = new Queue<object>(10);
            }
            else
            {
                this.readData = incomingData;
            }
        }

        public void SetReadStream(object[] incomingData, byte pos = 0)
        {
            this.readData = incomingData;
            this.currentItem = pos;
            this.write = false;
        }

        internal void ResetWriteStream()
        {
            writeData.Clear();
        }

        /// <summary>If true, this client should add data to the stream to send it.</summary>
        public bool isWriting
        {
            get { return this.write; }
        }

        /// <summary>If true, this client should read data send by another client.</summary>
        public bool isReading
        {
            get { return !this.write; }
        }

        /// <summary>Count of items in the stream.</summary>
        public int Count
        {
            get
            {
                return (this.isWriting) ? this.writeData.Count : this.readData.Length;
            }
        }

        /// <summary>Read next piece of data from the stream when isReading is true.</summary>
        public object ReceiveNext()
        {
            if (this.write)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
            }

            var obj = this.readData[this.currentItem];
            this.currentItem++;
            return obj;
        }
        public T ReceiveNext<T>()
        {
            if (this.write)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
            }

            var obj = this.readData[this.currentItem];
            this.currentItem++;
            var formatter = SerializeTypes.GetFormatter<T>();
            if (formatter == null) return (T)obj;
            return (T)formatter.Deserialize((byte[])obj);
        }

        /// <summary>Read next piece of data from the stream without advancing the "current" item.</summary>
        public object PeekNext()
        {
            if (this.write)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
            }

            object obj = this.readData[this.currentItem];
            //this.currentItem++;
            return obj;
        }

        public void SendNext(object obj)
        {
            if (!this.write)
            {
                Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
                return;
            }
            this.writeData.Enqueue(obj);
        }

        public object[] ToArray()
        {
            return this.isWriting ? this.writeData.ToArray() : this.readData;
        }

        public void Serialize<T>(ref T obj)
        {
            this.writeData.Enqueue(obj);
        }

        public bool DeSerialize<T>(ref T obj)
        {
            if (this.readData.Length > currentItem)
            {
                var formatter = SerializeTypes.GetFormatter<T>();
                if(formatter == null){
                    obj = (T)this.readData[this.currentItem];
                }else{
                    obj = (T)formatter.Deserialize((byte[])this.readData[this.currentItem]);
                }
                this.currentItem++;
                return true;
            }
            else
            {
                return false;
            }
        }
    }



    public class SceneManagerHelper
    {
        public static string ActiveSceneName
        {
            get
            {
            UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return s.name;
            }
        }

        public static int ActiveSceneBuildIndex
        {
            get
            {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            }
        }

#if UNITY_EDITOR
        public static string EditorActiveSceneName
        {
            get
            {
                return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            }
        }
#endif
    }

}
