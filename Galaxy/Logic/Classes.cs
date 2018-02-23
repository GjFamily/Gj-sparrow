using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Network;

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
                return null;
            }

            object obj = this.readData[this.currentItem];
            this.currentItem++;
            return obj;
        }

        /// <summary>Read next piece of data from the stream without advancing the "current" item.</summary>
        public object PeekNext()
        {
            if (this.write)
            {
                Debug.LogError("Error: you cannot read this stream that you are writing!");
                return null;
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

        /// <summary>Turns the stream into a new object[].</summary>
        public object[] ToArray()
        {
            return this.isWriting ? this.writeData.ToArray() : this.readData;
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref bool myBool)
        {
            if (this.write)
            {
                this.writeData.Enqueue(myBool);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    myBool = (bool)this.readData[currentItem];
                    this.currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref int myInt)
        {
            if (write)
            {
                this.writeData.Enqueue(myInt);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    myInt = (int)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref string value)
        {
            if (write)
            {
                this.writeData.Enqueue(value);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    value = (string)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref char value)
        {
            if (write)
            {
                this.writeData.Enqueue(value);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    value = (char)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref short value)
        {
            if (write)
            {
                this.writeData.Enqueue(value);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    value = (short)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref float obj)
        {
            if (write)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    obj = (float)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref NetworkPlayer obj)
        {
            if (write)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    obj = (NetworkPlayer)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Vector3 obj)
        {
            if (write)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    obj = (Vector3)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Vector2 obj)
        {
            if (write)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    obj = (Vector2)this.readData[currentItem];
                    currentItem++;
                }
            }
        }

        /// <summary>
        /// Will read or write the value, depending on the stream's isWriting value.
        /// </summary>
        public void Serialize(ref Quaternion obj)
        {
            if (write)
            {
                this.writeData.Enqueue(obj);
            }
            else
            {
                if (this.readData.Length > currentItem)
                {
                    obj = (Quaternion)this.readData[currentItem];
                    currentItem++;
                }
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
