using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Network;
using System.IO;

namespace Gj.Galaxy.Logic{
    public interface GameObservable
    {
        void OnSerializeEntity(StreamBuffer stream, MessageInfo info);
        void OnDeserializeEntity(StreamBuffer stream, MessageInfo info);
        void SetSyncParam(byte param);
    }

    public interface PrefabPool
    {
        GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation);

        void Destroy(GameObject gameObject);
    }

    /// <summary>
    /// Container class for info about a particular message, RPC or update.
    /// </summary>
    /// \ingroup publicApi
    public struct MessageInfo
    {
        private readonly long timeInt;
        /// <summary>The sender of a message / event. May be null.</summary>
        public readonly NetworkPlayer sender;
        public readonly NetworkEntity entity;

        public MessageInfo(NetworkPlayer player, NetworkEntity entity)
        {
            this.sender = player;
            this.timeInt = PeerClient.LocalTimestamp;
            this.entity = entity;
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

        /// <summary>
        /// Creates a stream and initializes it. Used by PUN internally.
        /// </summary>
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
            if(formatter == null){
                return (T)obj;
            }else{
                return (T)formatter.Deserialize((byte[])obj);
            }
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

        /// <summary>Add another piece of data to send it when isWriting is true.</summary>
        public void SendNext(object obj)
        {
            if (!this.write)
            {
                Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
                return;
            }
            this.writeData.Enqueue(obj);
        }
        public void SendNext<T>(T obj)
        {
            if (!this.write)
            {
                Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
                return;
            }
            var formatter = SerializeTypes.GetFormatter<T>();
            if (formatter == null)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                formatter.Serialize(stream, obj);
                this.writeData.Enqueue(stream.GetBuffer());
            }
        }

        /// <summary>Turns the stream into a new object[].</summary>
        public object[] ToArray()
        {
            return this.isWriting ? this.writeData.ToArray() : this.readData;
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref NetworkPlayer obj)
        {
            var formatter = PlayerSerializeFormatter.instance;
            formatter.Serialize(stream, obj);
            this.writeData.Enqueue(stream.GetBuffer());
        }

        public bool DeSerialize(out NetworkPlayer obj)
        {
            var formatter = PlayerSerializeFormatter.instance;
            if (this.readData.Length > currentItem)
            {
                obj = (NetworkPlayer)formatter.Deserialize((byte[])this.readData[currentItem]);
                currentItem++;
                return true;
            }else{
                obj = new NetworkPlayer(false, 0, "");
                return false;
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Vector3 obj)
        {
            var formatter = Vector3SerializeFormatter.instance;
            formatter.Serialize(stream, obj);
            this.writeData.Enqueue(stream.GetBuffer());
        }

        public bool DeSerialize(out Vector3 obj)
        {
            var formatter = Vector3SerializeFormatter.instance;
            if (this.readData.Length > currentItem)
            {
                obj = (Vector3)formatter.Deserialize((byte[])this.readData[currentItem]);
                currentItem++;
                return true;
            }else{
                obj = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Vector2 obj)
        {
            var formatter = Vector2SerializeFormatter.instance;
            formatter.Serialize(stream, obj);
            this.writeData.Enqueue(stream.GetBuffer());
        }

        public bool DeSerialize(out Vector2 obj)
        {
            var formatter = Vector2SerializeFormatter.instance;
            if (this.readData.Length > currentItem)
            {
                obj = (Vector2)formatter.Deserialize((byte[])this.readData[currentItem]);
                currentItem++;
                return true;
            }else{
                obj = Vector2.zero;
                return false;
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Quaternion obj)
        {
            var formatter = QuaternionSerializeFormatter.instance;
            formatter.Serialize(stream, obj);
            this.writeData.Enqueue(stream.GetBuffer());
        }

        public bool DeSerialize(out Quaternion obj)
        {
            var formatter = QuaternionSerializeFormatter.instance;
            if (this.readData.Length > currentItem)
            {
                obj = (Quaternion)formatter.Deserialize((byte[])this.readData[currentItem]);
                currentItem++;
                return true;
            }else{
                obj = new Quaternion();
                return false;
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize<T>(ref T obj)
        {
            this.writeData.Enqueue(obj);
        }

        public bool DeSerialize<T>(ref T obj)
        {
            if (this.readData.Length > currentItem)
            {
                obj = (T)this.readData[this.currentItem];
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
