using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Gj.Galaxy.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gj.Galaxy.Logic{
    public class NetworkEntity : MonoBehaviour
    {
        // 拥有人id
        public int ownerId;
        // 创建人id
        public int creatorId;

        public byte group = 0;
        // NOTE: this is now an integer because unity won't serialize short (needed for instantiation). we SEND only a short though!
        // NOTE: prefabs have a prefixBackup of -1. this is replaced with any currentLevelPrefix that's used at runtime. instantiated GOs get their prefix set pre-instantiation (so those are not -1 anymore)
        public int prefix
        {
            get
            {
                if (this.prefixBackup == -1)
                {
                    this.prefixBackup = GameConnect.currentLevelPrefix;
                }

                return this.prefixBackup;
            }
            set { this.prefixBackup = value; }
        }

        // this field is serialized by unity. that means it is copied when instantiating a persistent obj into the scene
        public int prefixBackup = -1;

        protected internal bool mixedModeIsReliable = false;

        /// <summary>
        /// Flag to check if ownership of this photonView was set during the lifecycle. Used for checking when joining late if event with mismatched owner and sender needs addressing.
        /// </summary>
        /// <value><c>true</c> if owner ship was transfered; otherwise, <c>false</c>.</value>
        public bool OwnerShipWasTransfered;

        //public object[] instantiationData
        //{
        //    get
        //    {
        //        if (!this.didAwake)
        //        {
        //            // even though viewID and instantiationID are setup before the GO goes live, this data can't be set. as workaround: fetch it if needed
        //            this.instantiationDataField = GameConnect.FetchInstantiationData(this.instantiationId);
        //        }
        //        return this.instantiationDataField;
        //    }
        //    set { this.instantiationDataField = value; }
        //}

        //internal object[] instantiationDataField;

        protected internal object[] lastOnSerializeDataSent = null;

        protected internal object[] lastOnSerializeDataReceived = null;

        public EntitySynchronization synchronization;

        public OwnershipOption ownershipTransfer = OwnershipOption.Fixed;

        public List<Component> ObservedComponents = new List<Component>();

        [SerializeField]
        private int idField = 0;
    
        public int entityId
        {
            get { return this.idField; }
            set
            {
                bool register = this.didAwake && this.idField == 0;

                this.idField = value;

                if (register)
                {
                    GameConnect.RegisterEntity(this);
                }
            }
        }

        public int instantiationId; 
        /// <summary>True if the PhotonView was loaded with the scene (game object) or instantiated with InstantiateSceneObject.</summary>
        /// <remarks>
        /// Scene objects are not owned by a particular player but belong to the scene. Thus they don't get destroyed when their
        /// creator leaves the game and the current Master Client can control them (whoever that is).
        /// The ownerId is 0 (player IDs are 1 and up).
        /// </remarks>
        public bool isScene
        {
            get { return this.creatorId < 0; }
        }

        public GamePlayer owner
        {
            get
            {
                return GameConnect.Room.Find(this.ownerId);
            }
        }

        public int OwnerId
        {
            get { return this.ownerId; }
        }

        public bool isOwnerActive
        {
            get { return this.ownerId != 0 && GameConnect.Room.mPlayers.ContainsKey(this.ownerId); }
        }

        public int CreatorId
        {
            get { return this.creatorId; }
        }

        public bool isMine
        {
            get
            {
                return SyncId == GameConnect.Room.LocalClientId;
            }
        }

        public int SyncId
        {
            get
            {
                return this.ownerId > 0 ? this.ownerId : Math.Abs(creatorId);
            }
        }

        protected internal bool didAwake;

        [SerializeField]
        protected internal bool isRuntimeInstantiated;

        protected internal bool removedFromLocalList;

        //internal MonoBehaviour[] RpcMonoBehaviours;

        protected internal void Awake()
        {
            if (this.entityId != 0)
            {
                // registration might be too late when some script (on this GO) searches this view BUT GetPhotonView() can search ALL in that case
                GameConnect.RegisterEntity(this);
                //this.instantiationDataField = GameConnect.FetchInstantiationData(this.instantiationId);
            }

            this.didAwake = true;
        }

        internal int OnTransferOwnership(int newOwnerId)
        {
            int _oldOwnerID = ownerId;
            OwnerShipWasTransfered = true;
            ownerId = newOwnerId;
            return _oldOwnerID;
        }

        protected internal void OnDestroy()
        {
            if (!this.removedFromLocalList)
            {
                bool wasInList = GameConnect.LocalCleanEntity(this);

                if (wasInList && this.instantiationId > 0 && !Handler.AppQuits && PeerClient.logLevel >= Network.LogLevel.Info)
                {
                    Debug.Log("instantiated '" + this.gameObject.name + "' got destroyed by engine. This is OK when loading levels. Otherwise use: Destroy().");
                }
            }
        }

        public const int SyncViewId = 0;
        public const int SyncCreatorId = 1;
        public const int SyncCompressed = 2;
        public const int SyncNullValues = 3;
        public const int SyncFirstValue = 4;

        internal object[] OnSerializeWrite(StreamBuffer stream, GamePlayer player)
        {
            if (synchronization == EntitySynchronization.Off)
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


            object[] currentValues = stream.ToArray();
            currentValues[0] = entityId;
            currentValues[1] = creatorId;
            currentValues[2] = false;
            currentValues[3] = null;

            if (synchronization == EntitySynchronization.Unreliable)
            {
                return currentValues;
            }

            // ViewSynchronization: Off, Unreliable, UnreliableOnChange, ReliableDeltaCompressed
            if (synchronization == EntitySynchronization.UnreliableOnChange)
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

                return currentValues;
            }

            if (synchronization == EntitySynchronization.Reliable)
            {
                object[] dataToSend = DeltaCompressionWrite(lastOnSerializeDataSent, currentValues);

                lastOnSerializeDataSent = currentValues;

                return dataToSend;
            }

            return null;
        }

        internal void OnSerializeRead(StreamBuffer stream, GamePlayer player, object[] data, short correctPrefix)
        {
            if (prefix > 0 && correctPrefix != prefix)
            {
                Debug.LogError("Received OnSerialization for view ID " + this + " with prefix " + correctPrefix + ". Our prefix is " + prefix);
                return;
            }
            if (synchronization == EntitySynchronization.Reliable)
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

            compressedContent[SyncViewId] = currentContent[SyncViewId];
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
                    observable.OnDeserializeEntity(stream, info);
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
                    observable.OnSerializeEntity(stream, info);
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

        //public void RefreshRpcMonoBehaviourCache()
        //{
        //    this.RpcMonoBehaviours = this.GetComponents<MonoBehaviour>();
        //}

        //public void RPC(string methodName, SyncTargets target, params object[] parameters)
        //{
        //    GameConnect.RPC(this, methodName, target, parameters);
        //}

        public static NetworkEntity Get(Component component)
        {
            return component.GetComponent<NetworkEntity>();
        }

        public static NetworkEntity Get(GameObject gameObj)
        {
            return gameObj.GetComponent<NetworkEntity>();
        }

        public override string ToString()
        {
            return string.Format("View {0} on {1} {2}", this.entityId, (this.gameObject != null) ? this.gameObject.name : "GO==null", (this.isScene) ? "(scene)" : string.Empty);
        }
    }
}
