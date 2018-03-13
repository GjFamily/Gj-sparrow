using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gj.Galaxy.Logic{
    public class NetworkEntity : MonoBehaviour
    {
        public int ownerId;

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

        public object[] instantiationData
        {
            get
            {
                if (!this.didAwake)
                {
                    // even though viewID and instantiationID are setup before the GO goes live, this data can't be set. as workaround: fetch it if needed
                    this.instantiationDataField = GameConnect.FetchInstantiationData(this.instantiationId);
                }
                return this.instantiationDataField;
            }
            set { this.instantiationDataField = value; }
        }

        internal object[] instantiationDataField;

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
            get { return this.CreatorActorNr == 0; }
        }

        public NetworkPlayer owner
        {
            get
            {
                return GameConnect.Room.Find(this.ownerId);
            }
        }

        public int OwnerActorNr
        {
            get { return this.ownerId; }
        }

        public bool isOwnerActive
        {
            get { return this.ownerId != 0 && GameConnect.Room.mActors.ContainsKey(this.ownerId); }
        }

        public int CreatorActorNr
        {
            get { return this.idField / GameConnect.MAX_ENTITY_IDS; }
        }

        public bool isMine
        {
            get
            {
                return (this.ownerId == GameConnect.Room.LocalClientId) || (!this.isOwnerActive && GameConnect.isMasterClient);
            }
        }

        protected internal bool didAwake;

        [SerializeField]
        protected internal bool isRuntimeInstantiated;

        protected internal bool removedFromLocalList;

        internal MonoBehaviour[] RpcMonoBehaviours;

        protected internal void Awake()
        {
            if (this.entityId != 0)
            {
                // registration might be too late when some script (on this GO) searches this view BUT GetPhotonView() can search ALL in that case
                GameConnect.RegisterEntity(this);
                this.instantiationDataField = GameConnect.FetchInstantiationData(this.instantiationId);
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

        public void RefreshRpcMonoBehaviourCache()
        {
            this.RpcMonoBehaviours = this.GetComponents<MonoBehaviour>();
        }

        public void RPC(string methodName, SyncTargets target, params object[] parameters)
        {
            GameConnect.RPC(this, methodName, target, parameters);
        }

        public static NetworkEntity Get(Component component)
        {
            return component.GetComponent<NetworkEntity>();
        }

        public static NetworkEntity Get(GameObject gameObj)
        {
            return gameObj.GetComponent<NetworkEntity>();
        }

        public static NetworkEntity Find(int entityId)
        {
            return GameConnect.GetEntity(entityId);
        }

        public override string ToString()
        {
            return string.Format("View {0} on {1} {2}", this.entityId, (this.gameObject != null) ? this.gameObject.name : "GO==null", (this.isScene) ? "(scene)" : string.Empty);
        }
    }

}
