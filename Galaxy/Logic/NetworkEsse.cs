using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Gj.Galaxy.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gj.Galaxy.Logic{
    public interface EsseBehaviour
    {
        void InitSync(NetworkEsse esse);
        bool GetData(StreamBuffer stream);
        void UpdateData(StreamBuffer stream);
        void OnOwnership(GamePlayer oldPlayer, GamePlayer newPlayer);
        void OnCommand(GamePlayer player, Dictionary<byte, object> data);
        void OnSurvey(Dictionary<byte, object> data);
    }
    public class NetworkEsse : MonoBehaviour
    {
        // 拥有人id
        public string ownerId
        {
            get
            {
                return owner != null ? owner.UserId : "";
            }
        }

        internal GamePlayer owner;
        // 创建人id
        public string creatorId;

        internal string hash;

        internal string group = "";
        public string Group
        {
            get
            {
                return group;
            }
            set
            {
                AreaConnect.ChangeLocation(this, value, level);
            }
        }

        internal byte level = 0;
        public byte Level
        {
            get
            {
                return Level;
            }
            set
            {
                AreaConnect.ChangeLocation(this, group, value);
            }
        }

        protected internal bool mixedModeIsReliable = false;

        public bool OwnerShipWasTransfered;

        protected internal EsseBehaviour behaviour;

        [SerializeField]
        protected internal bool isRuntimeInstantiated;

        protected internal bool removedFromLocalList;

        protected internal string prefabs;

        protected internal UInt16 version = 0;

        protected internal object[] lastOnSerializeDataSent = null;

        protected internal object[] lastOnSerializeDataReceived = null;

        public Synchronization synchronization;

        public OwnershipOption ownershipTransfer = OwnershipOption.Fixed;

        public List<Component> ObservedComponents = new List<Component>();

        public string Id
        {
            get { return this.hash; }
            set
            {
                this.hash = value;

                if (behaviour == null)
                {
                    behaviour = GetComponent<EsseBehaviour>() as EsseBehaviour;
                    // todo behaviour null
                }
                if (behaviour != null)
                    behaviour.InitSync(this);
                AreaConnect.Register(this);
            }
        }

        public string OwnerId
        {
            get { return owner != null ? owner.UserId : ""; }
        }

        public string CreatorId
        {
            get { return this.creatorId; }
        }

        public string SyncId
        {
            get
            {
                return this.ownerId != "" ? this.ownerId : this.creatorId;
            }
        }

        public bool isMine
        {
            get
            {
                return SyncId == AreaConnect.localId;
            }
        }

        protected internal bool GetData(StreamBuffer stream)
        {
            if (behaviour != null)
                return behaviour.GetData(stream);
            else
                return false;
        }

        protected internal void UpdateData(StreamBuffer stream)
        {
            if (behaviour != null)
                behaviour.UpdateData(stream);
        }

		public void UpdateInfo()
		{
            AreaConnect.ChangeInfo(this);
		}

        internal void OnTransferOwnership(GamePlayer newPlayer)
        {
            behaviour.OnOwnership(owner, newPlayer);
            owner = newPlayer;
        }

        public void Takeover(Action<bool> callback)
        {
            if (ownerId == AreaConnect.localId)
            {
                callback(true);
            }
            else
            {
                AreaConnect.Ownership(this, false, callback);
            }
        }

        public void Giveout(Action<bool> callback)
        {
            if (ownerId != AreaConnect.localId)
            {
                callback(false);
            }
            else
            {
                AreaConnect.Ownership(this, true, (b)=>{
                    callback(!b);
                });
            }
        }

        public void Command(Dictionary<byte, object> data, Action callback)
        {
            AreaConnect.Command(this, callback, data);
        }

        internal void OnCommand(GamePlayer player, Dictionary<byte, object> data)
        {
            behaviour.OnCommand(player, data);
        }

        internal void OnSurvey(Dictionary<byte, object> data)
        {
            behaviour.OnSurvey(data);
        }
        public void Destroy()
        {
            AreaConnect.Destroy(this);
        }

        protected internal void OnDestroy()
        {
            if (!this.removedFromLocalList)
            {
                bool wasInList = AreaConnect.LocalClean(this);

                if (wasInList && !Handler.AppQuits && PeerClient.logLevel >= Network.LogLevel.Info)
                {
                    Debug.Log("instantiated '" + this.gameObject.name + "' got destroyed by engine. This is OK when loading levels. Otherwise use: Destroy().");
                }
            }
        }

        // 关联数据对象，
        public void Relation(string prefabName, byte relation, bool isOwner)
        {
            AreaConnect.RelationInstance(this, prefabName, relation, this.gameObject, isOwner);
        }

        public const int SyncHash = 0;
        public const int SyncCompressed = 1;
        public const int SyncNullValues = 2;
        public const int SyncVersion = 3;
        public const int SyncFirstValue = 4;

        internal object[] OnSerializeWrite(StreamBuffer stream, GamePlayer player)
        {
            if (synchronization == Synchronization.Off)
            {
                return null;
            }

            stream.ResetWriteStream();
            stream.SendNext(null);
            stream.SendNext(null);
            stream.SendNext(null);
            stream.SendNext(null);

            // each view creates a list of values that should be sent
            MessageInfo info = new MessageInfo(player, this);
            Serialize(stream, info);

            // check if there are actual values to be sent (after the "header" of viewId, (bool)compressed and (int[])nullValues)
            if (stream.Count <= SyncFirstValue)
            {
                return null;
            }
            //Debug.Log(stream.Count);

            object[] currentValues = stream.ToArray();
            currentValues[SyncHash] = hash;
            currentValues[SyncCompressed] = false;
            currentValues[SyncNullValues] = null;

            if (synchronization == Synchronization.Unreliable)
            {
                currentValues[SyncVersion] = version++;
                return SerializeStream(ref currentValues);
            }

            if (synchronization == Synchronization.UnreliableOnChange)
            {
                if (AlmostEquals(currentValues, lastOnSerializeDataSent))
                {
                    if (mixedModeIsReliable)
                    {
                        return null;
                    }

                    mixedModeIsReliable = true;
                    lastOnSerializeDataSent = currentValues;
                }
                else
                {
                    mixedModeIsReliable = false;
                    lastOnSerializeDataSent = currentValues;
                }

                currentValues[SyncVersion] = version++;
                return SerializeStream(ref currentValues);
            }

            if (synchronization == Synchronization.Reliable)
            {
                object[] dataToSend = DeltaCompressionWrite(lastOnSerializeDataSent, currentValues);

                lastOnSerializeDataSent = currentValues;

                if (dataToSend == null) return null;

                dataToSend[SyncVersion] = version++;
                return SerializeStream(ref dataToSend);
            }

            return null;
        }

        internal void OnSerializeRead(StreamBuffer stream, GamePlayer player, object[] data)
        {
            var v = data[SyncVersion].ConverUInt16();
            if (v < version) return;
            if (synchronization == Synchronization.Reliable)
            {
                object[] uncompressed = this.DeltaCompressionRead(lastOnSerializeDataReceived, data);
                if (uncompressed == null)
                {
                    return;
                }

                // store last received values (uncompressed) for delta-compression usage
                lastOnSerializeDataReceived = uncompressed;
                data = uncompressed;
            }

            MessageInfo info = new MessageInfo(player, this);
            stream.SetReadStream(data, SyncFirstValue);

            Deserialize(stream, info);
            version = v;
        }

        private object[] DeltaCompressionWrite(object[] previousContent, object[] currentContent)
        {
            if (currentContent == null || previousContent == null || previousContent.Length != currentContent.Length)
            {
                return currentContent;  // the current data needs to be sent (which might be null)
            }

            if (currentContent.Length <= SyncFirstValue)
            {
                return null;  // this send doesn't contain values (except the "headers"), so it's not being sent
            }

            object[] compressedContent = previousContent;   // the previous content is no longer needed, once we compared the values!
            compressedContent[SyncCompressed] = false;
            int compressedValues = 0;

            Queue<int> valuesThatAreChangedToNull = null;
            for (int index = SyncFirstValue; index < currentContent.Length; index++)
            {
                object newObj = currentContent[index];
                object oldObj = previousContent[index];
                if (AlmostEquals(newObj, oldObj))
                {
                    // compress (by using null, instead of value, which is same as before)
                    compressedValues++;
                    compressedContent[index] = null;
                }
                else
                {
                    compressedContent[index] = newObj;

                    // value changed, we don't replace it with null
                    // new value is null (like a compressed value): we have to mark it so it STAYS null instead of being replaced with previous value
                    if (newObj == null)
                    {
                        if (valuesThatAreChangedToNull == null)
                        {
                            valuesThatAreChangedToNull = new Queue<int>(currentContent.Length);
                        }
                        valuesThatAreChangedToNull.Enqueue(index);
                    }
                }
            }

            // Only send the list of compressed fields if we actually compressed 1 or more fields.
            if (compressedValues > 0)
            {
                if (compressedValues == currentContent.Length - SyncFirstValue)
                {
                    // all values are compressed to null, we have nothing to send
                    return null;
                }

                compressedContent[SyncCompressed] = true;
                if (valuesThatAreChangedToNull != null)
                {
                    compressedContent[SyncNullValues] = valuesThatAreChangedToNull.ToArray(); // data that is actually null (not just cause we didn't want to send it)
                }
            }

            compressedContent[SyncHash] = currentContent[SyncHash];
            return compressedContent;    // some data was compressed but we need to send something
        }

        private object[] DeltaCompressionRead(object[] lastOnSerializeDataReceived, object[] incomingData)
        {
            if ((bool)incomingData[SyncCompressed] == false)
            {
                // index 1 marks "compressed" as being true.
                return incomingData;
            }

            // Compression was applied (as data[1] == true)
            // we need a previous "full" list of values to restore values that are null in this msg. else, ignore this
            if (lastOnSerializeDataReceived == null)
            {
                return null;
            }

            int[] indexesThatAreChangedToNull = incomingData[(byte)2] as int[];
            for (int index = SyncFirstValue; index < incomingData.Length; index++)
            {
                if (indexesThatAreChangedToNull != null && indexesThatAreChangedToNull.Contains(index))
                {
                    continue;   // if a value was set to null in this update, we don't need to fetch it from an earlier update
                }
                if (incomingData[index] == null)
                {
                    // we replace null values in this received msg unless a index is in the "changed to null" list
                    object lastValue = lastOnSerializeDataReceived[index];
                    incomingData[index] = lastValue;
                }
            }

            return incomingData;
        }

        private static bool AlmostEquals(object[] lastData, object[] currentContent)
        {
            if (lastData == null && currentContent == null)
            {
                return true;
            }

            if (lastData == null || currentContent == null || (lastData.Length != currentContent.Length))
            {
                return false;
            }

            for (int index = 0; index < currentContent.Length; index++)
            {
                object newObj = currentContent[index];
                object oldObj = lastData[index];
                if (!AlmostEquals(newObj, oldObj))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if both objects are almost identical.
        /// Used to check whether two objects are similar enough to skip an update.
        /// </summary>
        private static bool AlmostEquals(object one, object two)
        {
            if (one == null || two == null)
            {
                return one == null && two == null;
            }

            if (!one.Equals(two))
            {
                // if A is not B, lets check if A is almost B
                if (one is Vector3)
                {
                    Vector3 a = (Vector3)one;
                    Vector3 b = (Vector3)two;
                    if (a.AlmostEquals(b, PeerClient.precisionForVectorSynchronization))
                    {
                        return true;
                    }
                }
                else if (one is Vector2)
                {
                    Vector2 a = (Vector2)one;
                    Vector2 b = (Vector2)two;
                    if (a.AlmostEquals(b, PeerClient.precisionForVectorSynchronization))
                    {
                        return true;
                    }
                }
                else if (one is Quaternion)
                {
                    Quaternion a = (Quaternion)one;
                    Quaternion b = (Quaternion)two;
                    if (a.AlmostEquals(b, PeerClient.precisionForQuaternionSynchronization))
                    {
                        return true;
                    }
                }
                else if (one is float)
                {
                    float a = (float)one;
                    float b = (float)two;
                    if (a.AlmostEquals(b, PeerClient.precisionForFloatSynchronization))
                    {
                        return true;
                    }
                }

                // one does not equal two
                return false;
            }

            return true;
        }

        private static object[] SerializeStream(ref object[] data)
        {
            SerializeFormatter formatter;
            var result = new object[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var r = data[i];
                if (r == null)
                {
                    result[i] = r;
                }
                else
                {
                    formatter = SerializeTypes.GetFormatter(r.GetType());
                    if (formatter == null)
                    {
                        result[i] = r;
                    }
                    else
                    {
                        result[i] = formatter.Serialize(r);
                    }
                }
            }
            return result;
        }

        public void BindComponent(Component observable)
        {
            ObservedComponents.Add(observable);
            var r = observable as GameObservable;
            if (r != null)
                r.Bind(this);
        }

        public void Serialize(StreamBuffer stream, MessageInfo info)
        {
            if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
            {
                for (int i = 0; i < this.ObservedComponents.Count; ++i)
                {
                    SerializeComponent(this.ObservedComponents[i], stream, info);
                }
            }
        }

        public void Deserialize(StreamBuffer stream, MessageInfo info)
        {
            if (this.ObservedComponents != null && this.ObservedComponents.Count > 0)
            {
                for (int i = 0; i < this.ObservedComponents.Count; ++i)
                {
                    DeserializeComponent(this.ObservedComponents[i], stream, info);
                }
            }
        }

        protected internal void DeserializeComponent(Component component, StreamBuffer stream, MessageInfo info)
        {
            if (component == null)
            {
                return;
            }

            // Use incoming data according to observed type
            if (component is MonoBehaviour)
            {
                GameObservable observable = component as GameObservable;
                if (observable != null)
                {
                    observable.OnDeserialize(stream, info);
                }
                else
                {
                    Debug.LogError("The observed monobehaviour (" + component.name + ")  does not implement GameObservable!");
                }
            }
            else
            {
                Debug.LogError("Type of observed is unknown when receiving.");
            }
        }

        protected internal void SerializeComponent(Component component, StreamBuffer stream, MessageInfo info)
        {
            if (component == null)
            {
                return;
            }

            if (component is MonoBehaviour)
            {
                GameObservable observable = component as GameObservable;
                if (observable != null)
                {
                    observable.OnSerialize(stream, info);
                }
                else
                {
                    Debug.LogError("The observed monobehaviour (" + component.name + ")  does not implement GameObservable!");
                }
            }
            else
            {
                Debug.LogError("Observed type is not serializable: " + component.GetType());
            }
        }

        public static NetworkEsse Get(Component component)
        {
            return component.GetComponent<NetworkEsse>();
        }

        public static NetworkEsse Get(GameObject gameObj)
        {
            return gameObj.GetComponent<NetworkEsse>();
        }

        public override string ToString()
        {
            return string.Format("Id {0} on {1}", this.Id, (this.gameObject != null) ? this.gameObject.name : "GO==null");
        }
    }
}
