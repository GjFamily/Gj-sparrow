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

    public enum NamespaceId
    {
        Auth = 0,
        Scene = 1,
        Room = 2,
        Team = 3
    }

    public static class EncryptionDataParameters
    {
        /// <summary>
        /// Key for encryption mode
        /// </summary>
        public const byte Mode = 0;
        /// <summary>
        /// Key for first secret
        /// </summary>
        public const byte Secret1 = 1;
        /// <summary>
        /// Key for second secret
        /// </summary>
        public const byte Secret2 = 2;
    }

    public enum Synchronization : byte { Off, Fixed, Manual }

    /// <summary>
    /// Options to define how Ownership Transfer is handled per PhotonView.
    /// </summary>
    /// <remarks>
    /// This setting affects how RequestOwnership and TransferOwnership work at runtime.
    /// </remarks>
    public enum OwnershipOption
    {
        /// <summary>
        /// Ownership is fixed. Instantiated objects stick with their creator.
        /// </summary>
        Fixed,
        /// <summary>
        /// Ownership can be requested with PhotonView.RequestOwnership but the current owner has to agree to give up ownership.
        /// </summary>
        /// <remarks>The current owner has to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.</remarks>
        Request
    }

    public struct MessageInfo
    {
        private readonly long timeInt;
        /// <summary>The sender of a message / event. May be null.</summary>
        public readonly NetworkEsse esse;

        public MessageInfo(NetworkEsse esse)
        {
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
            return string.Format("[MessageInfo: Senttime={0}]", this.timestamp);
        }
    }

	public class StreamBuffer:DataPacket
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
        
		public void Packet(Stream writer)
		{
			MessagePack.MessagePackSerializer.Serialize<object[]>(writer, ToArray());
		}
	}

	public class StreamQueue
    {
        #region Members

        private int m_SampleRate;
        private int m_SampleCount;
        private int m_ObjectsPerSample = -1;

        private float m_LastSampleTime = -Mathf.Infinity;
        private int m_LastFrameCount = -1;
        private int m_NextObjectIndex = -1;

        private List<object> m_Objects = new List<object>();

        private bool m_IsWriting;

        #endregion

        public StreamQueue(int sampleRate)
        {
            this.m_SampleRate = sampleRate;
        }

        private void BeginWritePackage()
        {
            if (Time.realtimeSinceStartup < this.m_LastSampleTime + 1f / this.m_SampleRate)
            {
                this.m_IsWriting = false;
                return;
            }

            if (this.m_SampleCount == 1)
            {
                this.m_ObjectsPerSample = this.m_Objects.Count;
            }
            else if (this.m_SampleCount > 1)
            {
                if (this.m_Objects.Count / this.m_SampleCount != this.m_ObjectsPerSample)
                {
                    Debug.LogWarning("The number of objects sent via a PhotonStreamQueue has to be the same each frame");
                    Debug.LogWarning("Objects in List: " + this.m_Objects.Count + " / Sample Count: " + this.m_SampleCount + " = " + (this.m_Objects.Count / this.m_SampleCount) + " != " + this.m_ObjectsPerSample);
                }
            }

            this.m_IsWriting = true;
            this.m_SampleCount++;
            this.m_LastSampleTime = Time.realtimeSinceStartup;

        }

        public void Reset()
        {
            this.m_SampleCount = 0;
            this.m_ObjectsPerSample = -1;

            this.m_LastSampleTime = -Mathf.Infinity;
            this.m_LastFrameCount = -1;

            this.m_Objects.Clear();
        }

        public void SendNext(object obj)
        {
            if (Time.frameCount != this.m_LastFrameCount)
            {
                BeginWritePackage();
            }

            this.m_LastFrameCount = Time.frameCount;

            if (this.m_IsWriting == false)
            {
                return;
            }

            this.m_Objects.Add(obj);
        }

        public bool HasQueuedObjects()
        {
            return this.m_NextObjectIndex != -1;
        }

        public object ReceiveNext()
        {
            if (this.m_NextObjectIndex == -1)
            {
                return null;
            }

            if (this.m_NextObjectIndex >= this.m_Objects.Count)
            {
                this.m_NextObjectIndex -= this.m_ObjectsPerSample;
            }

            return this.m_Objects[this.m_NextObjectIndex++];
        }

        public void Serialize(StreamBuffer stream)
        {
            // TODO: find a better solution for this:
            // the "if" is a workaround for packages which have only 1 sample/frame. in that case, SendNext didn't set the obj per sample.
            if (m_Objects.Count > 0 && this.m_ObjectsPerSample < 0)
            {
                this.m_ObjectsPerSample = m_Objects.Count;
            }

            stream.SendNext(this.m_SampleCount);
            stream.SendNext(this.m_ObjectsPerSample);

            for (int i = 0; i < this.m_Objects.Count; ++i)
            {
                stream.SendNext(this.m_Objects[i]);
            }

            this.m_Objects.Clear();
            this.m_SampleCount = 0;
        }

        public void Deserialize(StreamBuffer stream)
        {
            this.m_Objects.Clear();

            this.m_SampleCount = (int)stream.ReceiveNext();
            this.m_ObjectsPerSample = (int)stream.ReceiveNext();

            for (int i = 0; i < this.m_SampleCount * this.m_ObjectsPerSample; ++i)
            {
                this.m_Objects.Add(stream.ReceiveNext());
            }

            if (this.m_Objects.Count > 0)
            {
                this.m_NextObjectIndex = 0;
            }
            else
            {
                this.m_NextObjectIndex = -1;
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
