using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;
using System.Reflection;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic
{
    //todo 离线模式
    internal class GameEvent
    {
        public const byte Ready = 1;
        public const byte ReadyAll = 2;
        public const byte AssignPlayer = 3;
        public const byte Init = 4;
        public const byte InitAll = 5;
        public const byte RPC = 6;
        public const byte Instance = 7;
        public const byte Destory = 8;
        public const byte Serialize = 9;
        public const byte Ownership = 10;
        public const byte Finish = 11;
        public const byte Exit = 12;
        public const byte Join = 13;
        public const byte Leave = 14;
        public const byte ChangeRoom = 15;
        public const byte ChangePlayer = 16;
        public const byte ChangeGroups = 17;
        public const byte Sync = 18;
    }

    internal enum GameStage
    {
        None,
        Connect,
        Join,
        Ready,
        ReadyAll,
        InitScene,
        InitPlayer,
        Start,
        Finish
    }

    public interface GameDelegate
    {
        void OnFinish(bool exit, Dictionary<string, object> result);
        void SceneInit(Action callback);
        void PlayerInit(Action callback);
        void OnStart();
        void OnLeaveGame();
        void OnOwnership(NetworkEntity entity, NetworkPlayer oldPlayer);
        GameObject OnInstance(string prefabName);
    }
    public interface GameRoomDelegate
    {
        void OnRoom();
        void OnFail(string reason);
        void OnRoomChange(Hashtable props);
        void OnPlayerJoin(NetworkPlayer player);
        void OnPlayerLeave(NetworkPlayer player);
        void OnPlayerRejoin(NetworkPlayer player);
        void OnPlayerChange(NetworkPlayer player, Hashtable props);
        void OnReadyPlayer(NetworkPlayer player);
        //void OnInitPlayer(NetworkPlayer player, int process);
        //void OnInitScene(int process);
        void OnReadyAll();
    }

    public class GameConnect : NamespaceListener
    {
        public static readonly int MAX_ENTITY_IDS = 1000; // VIEW & PLAYER LIMIT CAN BE EASILY CHANGED, SEE DOCS

        public static GameDelegate Delegate;
        public static GameRoomDelegate RoomDelegate;
        private static Namespace n;
        public static GameRoom Room;
        private static GameConnect listener;

        protected static internal Dictionary<int, NetworkEntity> entityList = new Dictionary<int, NetworkEntity>(); //TODO: make private again

        public static NetworkPlayer masterClient
        {
            get
            {
                if (inRoom)
                {
                    return Room.masterClient;
                }
                else
                {
                    return null;
                }
            }
        }
        public static bool isMasterClient
        {
            get
            {
                if (PeerClient.offlineMode)
                {
                    return true;
                }
                else if (inRoom)
                {
                    return Room.isMasterClient;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool inRoom
        {
            get
            {
                return Room != null;
            }
        }

        internal static int lastUsedViewSubId = 0;  // each player only needs to remember it's own (!) last used subId to speed up assignment
        internal static int lastUsedViewSubIdScene = 0;  // per room, the master is able to instantiate GOs. the subId for this must be unique too
        internal static List<int> manuallyAllocatedEntityIds = new List<int>();

        static GameConnect()
        {
            n = SceneConnect.Of(SceneRoom.Game);
            n.compress = CompressType.Snappy;
            n.protocol = ProtocolType.Speed;
            n.messageQueue = MessageQueue.On;
            listener = new GameConnect();
            n.listener = listener;
        }
        #region flow
        // SceneConnect收到游戏房间后，执行进入游戏
        public static void JoinGame(string gameName, GameRoomDelegate d = null)
        {
            if (stage != GameStage.None)
                throw new Exception("Game is going");
            // 需要设定玩家委托
            // 会触发游戏进入成功，开始接受相关房间事件
            // 玩家加入，玩家修改信息
            if (RoomDelegate == null && d == null)
                throw new Exception("need Room Delegate");
            Room = new GameRoom();

            n.Connect("game=" + gameName);
            stage = GameStage.Connect;
        }

        // 通知服务器已经能开始游戏
        public static void ReadyGame()
        {
            if (stage != GameStage.Join)
                throw new Exception("game stage need join");
            n.Emit(GameEvent.Ready, new object[] { Room.localPlayer.Id });
            Room.localPlayer.IsReady = true;
            stage = GameStage.Ready;
            // 会发送给其他玩家准备完毕事件
            // 会触发所有玩家准备完毕事件
        }

        //开始进行场景同步，需要已经进入必要的scene中
        public static void StartGame(GameDelegate d = null)
        {
            if (Delegate == null && d == null)
                throw new Exception("need Game Delegate");
            if (!Room.localPlayer.IsReady)
                throw new Exception("local player need ready");
            if (stage != GameStage.ReadyAll)
                throw new Exception("game stage need join");
            listener.ResetEntityOnSerialize();
            listener.LoadLevel();
            //会触发场景初始化及玩家初始化，以及开始游戏
            //PeerClient.isMessageQueueRunning = true;
            stage = GameStage.InitScene;
            if (Room.isMasterClient)
            {
                Delegate.SceneInit(()=>{
                    n.Emit(GameEvent.Init, new object[] { 0, lastUsedViewSubIdScene });
                    stage = GameStage.InitPlayer;
                    Delegate.PlayerInit(()=>{
                        n.Emit(GameEvent.Init, new object[] { Room.localPlayer.Id, lastUsedViewSubId });
                        Room.OnInit(Room.localPlayer.Id, lastUsedViewSubId);
                        stage = GameStage.Start;
                        Delegate.OnStart();
                    });
                });
            }else{
                stage = GameStage.InitPlayer;
                Delegate.PlayerInit(() => {
                    n.Emit(GameEvent.Init, new object[] { Room.localPlayer.Id, lastUsedViewSubId });
                    Room.OnInit(Room.localPlayer.Id, lastUsedViewSubId);
                    stage = GameStage.Start;
                    Delegate.OnStart();
                });
            }

            //PeerClient.isMessageQueueRunning = false;
        }

        // 准备退出同步
        public static void LeaveGame()
        {
            if (stage == GameStage.None)
                throw new Exception("need Join Room first");
            if (stage == GameStage.Finish)
            {
                n.Disconnect();
                listener.OnDisconnect();
            }
            else
            {
                n.Emit(GameEvent.Exit, null, (result) =>
                {
                    Delegate.OnFinish(true, (Dictionary<string, object>)result[0]);
                    n.Disconnect();
                    listener.OnDisconnect();
                });
            }

            //会触发游戏结束，（正常结束也会触发）
        }
        #endregion

        public void OnConnect(bool success)
        {
            if (!inRoom)
            {
                Debug.Log("connect is error, need in room");
                return;
            }
            if (!success)
            {
                RoomDelegate.OnFail("connect is fail");
            }
            else
            {
                stage = GameStage.Join;
            }
        }

        public void OnDisconnect()
        {
            ClearRoom();
            stage = GameStage.None;
        }

        public void OnReconnect(bool success)
        {
            if (success)
            {
                n.Emit(GameEvent.Sync, null);
            }
            else
            {
                RoomDelegate.OnFail("reconnect is fail");
            }
        }

        public void OnError(string message)
        {
            Debug.Log(message);
        }

        public object[] OnEvent(byte code, object[] param)
        {
            if (!inRoom)
            {
                Debug.Log("connect is error, need in room");
                return null;
            }
            switch (code)
            {
                case GameEvent.AssignPlayer:
                    Room.OnRoom((int)param[0]);
                    break;
                case GameEvent.Ready:
                    Room.OnReady((int)param[0]);
                    break;
                case GameEvent.ReadyAll:
                    stage = GameStage.ReadyAll;
                    Room.OnReadyAll();
                    break;
                case GameEvent.Init:
                    Room.OnInit((int)param[0], (int)param[1]);
                    break;
                case GameEvent.RPC:
                    listener.OnRpc((int)param[0], (Hashtable)param[1]);
                    break;
                case GameEvent.Instance:
                    listener.OnInstance((int)param[0], (Hashtable)param[1], null);
                    break;
                case GameEvent.Destory:
                    listener.OnDestory((int)param[0], (int)param[1]);
                    break;
                case GameEvent.Serialize:
                    listener.OnSerialize((int)param[0], (Hashtable)param[1]);
                    break;
                case GameEvent.Ownership:
                    listener.OnOwnership((int)param[0], (int)param[1]);
                    break;
                case GameEvent.Finish:
                    stage = GameStage.Finish;
                    Delegate.OnFinish(false, (Dictionary<string, object>)param[0]);
                    break;
                case GameEvent.Join:
                    Room.OnJoin((int)param[0], (string)param[1], (string)param[2], (Hashtable)param[3]);
                    break;
                case GameEvent.Leave:
                    Room.OnLeave((int)param[0]);
                    break;
                case GameEvent.ChangeRoom:
                    Room.OnChangeRoom((Hashtable)param[0]);
                    break;
                case GameEvent.ChangePlayer:
                    Room.OnChangePlayer((int)param[0], (Hashtable)param[1]);
                    break;
                default:
                    Debug.Log("GameEvent is error:");
                    break;
            }
            return null;
        }

        internal static void EmitRoom(Hashtable props)
        {
            n.Emit(GameEvent.ChangeRoom, new object[] { props });
        }

        internal static void EmitPlayer(int playerId, Hashtable props)
        {
            n.Emit(GameEvent.ChangePlayer, new object[] { playerId, props });
        }

        public static void Ownership(int entityId, int playerId)
        {
            n.Emit(GameEvent.Ownership, new object[] { entityId, playerId });   // All sends to all via server (including self)
        }

        public static void RegisterEntity(NetworkEntity netEntity)
        {
            if (!inRoom)
            {
                return;
            }

            if (netEntity.entityId == 0)
            {
                Debug.Log("NetworkEntity register is ignored, because entityId is 0. No id assigned yet to: " + netEntity);
                return;
            }

            NetworkEntity entity = null;
            bool isListed = entityList.TryGetValue(netEntity.entityId, out entity);
            if (isListed)
            {
                // if some other view is in the list already, we got a problem. it might be undestructible. print out error
                if (netEntity != entity)
                {
                    Debug.LogError(string.Format("NetworkEntity ID duplicate found: {0}. New: {1} old: {2}. Maybe one wasn't destroyed on scene load?! Check for 'DontDestroyOnLoad'. Destroying old entry, adding new.", netEntity.entityId, netEntity, entity));
                }
                else
                {
                    return;
                }

                listener.RemoveInstantiatedGO(entity.gameObject, true);
            }

            // Debug.Log("adding view to known list: " + netView);
            entityList.Add(netEntity.entityId, netEntity);
            //Debug.LogError("view being added. " + netView);   // Exit Games internal log

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Registered NetworkEntity: " + netEntity.entityId);
            }
        }

        public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data)
        {
            if (!inRoom)
            {
                Debug.LogError("Failed to Instantiate prefab: " + prefabName + ". Client should be in a room. Current connectionStateDetailed: " + PeerClient.client.State);
                return null;
            }

            GameObject prefabGo;
            //if (!UsePrefabCache || !PrefabCache.TryGetValue(prefabName, out prefabGo))
            //{
            //    prefabGo = (GameObject)Resources.Load(prefabName, typeof(GameObject));
            //    if (UsePrefabCache)
            //    {
            //        PrefabCache.Add(prefabName, prefabGo);
            //    }
            //}
            prefabGo = Delegate.OnInstance(prefabName);

            if (prefabGo == null)
            {
                Debug.LogError("Failed to Instantiate prefab: " + prefabName + ". Verify the Prefab is in a Resources folder (and not in a subfolder)");
                return null;
            }

            // a scene object instantiated with network visibility has to contain a PhotonView
            if (prefabGo.GetComponent<NetworkEntity>() == null)
            {
                Debug.LogError("Failed to Instantiate prefab:" + prefabName + ". Prefab must have a PhotonView component.");
                return null;
            }

            Component[] entitys = (Component[])prefabGo.GetEntitysInChildren();
            int[] entityIds = new int[entitys.Length];
            for (int i = 0; i < entityIds.Length; i++)
            {
                entityIds[i] = AllocateEntityId(Room.localPlayer.Id);
            }

            // Send to others, create info
            Hashtable instantiateEvent = listener.EmitInstantiate(Room.localPlayer.Id, prefabName, position, rotation, group, entityIds, data, false);

            // Instantiate the GO locally (but the same way as if it was done via event). This will also cache the instantiationId
            return listener.OnInstance(Room.localPlayer.Id, instantiateEvent, prefabGo);
        }

        public static GameObject InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data)
        {
            if (!inRoom)
            {
                Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". Client should be in a room. Current connectionStateDetailed: " + PeerClient.connected);
                return null;
            }

            if (!isMasterClient)
            {
                Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". Client is not the MasterClient in this room.");
                return null;
            }

            GameObject prefabGo;
            //if (!UsePrefabCache || !PrefabCache.TryGetValue(prefabName, out prefabGo))
            //{
            //    prefabGo = (GameObject)Resources.Load(prefabName, typeof(GameObject));
            //    if (UsePrefabCache)
            //    {
            //        PrefabCache.Add(prefabName, prefabGo);
            //    }
            //}
            prefabGo = Delegate.OnInstance(prefabName);

            if (prefabGo == null)
            {
                Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". Verify the Prefab is in a Resources folder (and not in a subfolder)");
                return null;
            }

            // a scene object instantiated with network visibility has to contain a PhotonView
            if (prefabGo.GetComponent<NetworkEntity>() == null)
            {
                Debug.LogError("Failed to InstantiateSceneObject prefab:" + prefabName + ". Prefab must have a NetworkEntity component.");
                return null;
            }

            Component[] entitys = (Component[])prefabGo.GetEntitysInChildren();
            int[] entityIds = AllocateSceneEntityIds(entitys.Length);

            if (entityIds == null)
            {
                Debug.LogError("Failed to InstantiateSceneObject prefab: " + prefabName + ". No ViewIDs are free to use. Max is: " + MAX_ENTITY_IDS);
                return null;
            }

            // Send to others, create info
            Hashtable instantiateEvent = listener.EmitInstantiate(0, prefabName, position, rotation, group, entityIds, data, true);

            // Instantiate the GO locally (but the same way as if it was done via event). This will also cache the instantiationId
            return listener.OnInstance(0, instantiateEvent, prefabGo);
        }

        public static void RelationInstance(string prefabName, GameObject prefabGo, byte group, object[] data)
        {
            // a scene object instantiated with network visibility has to contain a PhotonView
            if (prefabGo.GetComponent<NetworkEntity>() == null)
            {
                Debug.LogError("Failed to RelationInstance prefab:" + prefabName + ". Prefab must have a NetworkEntity component.");
                return ;
            }

            Component[] entitys = (Component[])prefabGo.GetEntitysInChildren();
            int[] entityIds = AllocateSceneEntityIds(entitys.Length);

            if (entityIds == null)
            {
                Debug.LogError("Failed to RelationInstance prefab: " + prefabName + ". No ViewIDs are free to use. Max is: " + MAX_ENTITY_IDS);
                return ;
            }
            int playerId = 0;
            if (stage != GameStage.InitScene){
                playerId = Room.localPlayer.Id;
            }
            Hashtable instantiateEvent = listener.EmitInstantiate(playerId, prefabName, prefabGo.transform.position, prefabGo.transform.rotation, group, entityIds, data, true);
            listener.OnInstance(playerId, instantiateEvent, prefabGo);
        }

        public static void Destroy(NetworkEntity target)
        {
            if (target != null)
            {
                listener.RemoveInstantiatedGO(target.gameObject, false);
            }
            else
            {
                Debug.LogError("Destroy(targetEntity) failed, cause targetEntity is null.");
            }
        }

        public static void Destroy(GameObject targetGo)
        {
            if (targetGo != null)
            {
                listener.RemoveInstantiatedGO(targetGo, false);
            }
            else
            {
                Debug.LogError("Destroy(targetGo) failed, cause targetEntity is null.");
            }
        }

        public static void DestroyPlayerObjects(NetworkPlayer targetPlayer)
        {
            if (targetPlayer == null)
            {
                Debug.LogError("DestroyPlayerObjects() failed, cause parameter 'targetPlayer' was null.");
            }
            else
            {
                DestroyPlayerObjects(targetPlayer.Id);
            }
        }

        public static void DestroyPlayerObjects(int targetPlayerId)
        {
            if (!inRoom)
            {
                return;
            }
            if (Room.isMasterClient || targetPlayerId == Room.localPlayer.Id)
            {
                listener.DestroyPlayerObjects(targetPlayerId, false);
            }
            else
            {
                Debug.LogError("DestroyPlayerObjects() failed, cause players can only destroy their own GameObjects. A Master Client can destroy anyone's. This is master: " + isMasterClient);
            }
        }

        public static void DestroyAll()
        {
            if (isMasterClient)
            {
                listener.DestroyAll(false);
            }
            else
            {
                Debug.LogError("Couldn't call DestroyAll() as only the master client is allowed to call this.");
            }
        }

        public static void RPC(NetworkEntity entity, string methodName, SyncTargets target, params object[] parameters)
        {
            if (Room == null)
            {
                Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
                return;
            }
            else
            {
                if (target == SyncTargets.MasterClient)
                {
                    listener.EmitRPC(entity, methodName, SyncTargets.Others, masterClient, parameters);
                }
                else
                {
                    listener.EmitRPC(entity, methodName, target, null, parameters);
                }
            }
        }

        /// <summary>
        /// Internal to send an RPC on given PhotonView. Do not call this directly but use: PhotonView.RPC!
        /// </summary>
        public static void RPC(NetworkEntity entity, string methodName, NetworkPlayer targetPlayer, params object[] parameters)
        {
            if (Room == null)
            {
                Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
                return;
            }

            listener.EmitRPC(entity, methodName, SyncTargets.Others, targetPlayer, parameters);
        }

        // todo: RPC Cache

        public static void SetInterestGroups(byte[] disableGroups, byte[] enableGroups)
        {
            if (disableGroups != null)
            {
                if (disableGroups.Length == 0)
                {
                    // a byte[0] should disable ALL groups in one step and before any groups are enabled. we do this locally, too.
                    allowedReceivingGroups.Clear();
                }
                else
                {
                    for (int index = 0; index < disableGroups.Length; index++)
                    {
                        byte g = disableGroups[index];
                        if (g <= 0)
                        {
                            Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + g + ". The group number should be at least 1.");
                            continue;
                        }

                        if (allowedReceivingGroups.Contains(g))
                        {
                            allowedReceivingGroups.Remove(g);
                        }
                    }
                }
            }

            if (enableGroups != null)
            {
                if (enableGroups.Length == 0)
                {
                    // a byte[0] should enable ALL groups in one step. we do this locally, too.
                    for (byte index = 0; index < byte.MaxValue; index++)
                    {
                        allowedReceivingGroups.Add(index);
                    }

                    // add this group separately to avoid an overflow exception in the previous loop
                    allowedReceivingGroups.Add(byte.MaxValue);
                }
                else
                {
                    for (int index = 0; index < enableGroups.Length; index++)
                    {
                        byte g = enableGroups[index];
                        if (g <= 0)
                        {
                            Debug.LogError("Error: PhotonNetwork.SetInterestGroups was called with an illegal group number: " + g + ". The group number should be at least 1.");
                            continue;
                        }

                        allowedReceivingGroups.Add(g);
                    }
                }
            }

            listener.EmitChangeGroups(disableGroups, enableGroups);
        }

        public void EmitChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            n.Emit(GameEvent.ChangeGroups, new object[] { groupsToRemove, groupsToAdd });
        }

        public static void SetSendingEnabled(byte[] disableGroups, byte[] enableGroups)
        {
            if (disableGroups != null)
            {
                for (int index = 0; index < disableGroups.Length; index++)
                {
                    byte g = disableGroups[index];
                    blockSendingGroups.Add(g);
                }
            }

            if (enableGroups != null)
            {
                for (int index = 0; index < enableGroups.Length; index++)
                {
                    byte g = enableGroups[index];
                    blockSendingGroups.Remove(g);
                }
            }
        }

        private static HashSet<byte> allowedReceivingGroups = new HashSet<byte>();
        private static HashSet<byte> blockSendingGroups = new HashSet<byte>();

        private readonly StreamBuffer readStream = new StreamBuffer(false, null);    // only used in OnSerializeRead()
        private readonly StreamBuffer pStream = new StreamBuffer(true, null);        // only used in OnSerializeWrite()
        private readonly Dictionary<int, Hashtable> dataPerGroupReliable = new Dictionary<int, Hashtable>();    // only used in RunViewUpdate()
        private readonly Dictionary<int, Hashtable> dataPerGroupUnreliable = new Dictionary<int, Hashtable>();  // only used in RunViewUpdate()

        static internal short currentLevelPrefix = 0;

        /// <summary>For automatic scene syncing, the loaded scene is put into a room property. This is the name of said prop.</summary>
        protected internal const string CurrentSceneProperty = "curScn";

        static internal GameStage stage = GameStage.None;

        public static bool UsePrefabCache = true;

        //internal IPunPrefabPool ObjectPool;

        public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

        private Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();

        private readonly Dictionary<string, int> rpcShortcuts;  // lookup "table" for the index (shortcut) of an RPC name

        private void ResetEntityOnSerialize()
        {
            foreach (NetworkEntity entity in entityList.Values)
            {
                entity.lastOnSerializeDataSent = null;
            }
        }

        internal Hashtable EmitInstantiate(int playerId, string prefabName, Vector3 position, Quaternion rotation, byte group, int[] viewIDs, object[] data, bool isGlobalObject)
        {
            // first viewID is now also the gameobject's instantiateId
            int instantiateId = viewIDs[0];   // LIMITS PHOTONVIEWS&PLAYERS

            //TODO: reduce hashtable key usage by using a parameter array for the various values
            Hashtable instantiateEvent = new Hashtable(); // This players info is sent via ActorID
            instantiateEvent[(byte)0] = prefabName;

            if (position != Vector3.zero)
            {
                instantiateEvent[(byte)1] = position;
            }

            if (rotation != Quaternion.identity)
            {
                instantiateEvent[(byte)2] = rotation;
            }

            if (group != 0)
            {
                instantiateEvent[(byte)3] = group;
            }

            // send the list of viewIDs only if there are more than one. else the instantiateId is the viewID
            if (viewIDs.Length > 1)
            {
                instantiateEvent[(byte)4] = viewIDs; // LIMITS PHOTONVIEWS&PLAYERS
            }

            if (data != null)
            {
                instantiateEvent[(byte)5] = data;
            }

            //instantiateEvent[(byte)6] = PeerClient.ServerTimestamp;
            instantiateEvent[(byte)6] = instantiateId;

            if (currentLevelPrefix > 0)
            {
                instantiateEvent[(byte)7] = currentLevelPrefix;
            }

            n.Emit(GameEvent.Instance, new object[] { playerId, instantiateEvent });
            return instantiateEvent;
        }

        private void EmitDestroyOfInstantiate(int instantiateId, int creatorId)
        {
            n.Emit(GameEvent.Destory, new object[] { 1, instantiateId });
        }

        private void EmitDestroyOfPlayer(int actorNr)
        {
            n.Emit(GameEvent.Destory, new object[] { 2, actorNr });
        }

        private void EmitDestroyOfAll()
        {
            n.Emit(GameEvent.Destory, new object[] { -1 });
        }
        #region OnEvent

        public void OnOwnership(int requestedEntityId, int newOwnerId)
        {
            NetworkPlayer newPlayer = Room.GetPlayerWithId(newOwnerId);
            NetworkEntity requestedEntity = GetEntity(requestedEntityId);
            NetworkPlayer oldPlayer = requestedEntity.owner;
            if (requestedEntity == null)
            {
                Debug.LogWarning("Can't find PhotonView of incoming OwnershipRequest. ViewId not found: " + requestedEntityId);
                return;
            }

            switch (requestedEntity.ownershipTransfer)
            {
                case OwnershipOption.Fixed:
                    Debug.LogWarning("Ownership mode == fixed. Ignoring request.");
                    break;
                case OwnershipOption.Takeover:
                    if (newPlayer == oldPlayer || (oldPlayer == null && requestedEntity.ownerId == Room.localPlayer.Id) || requestedEntity.ownerId == 0)
                    {
                        // a takeover is successful automatically, if taken from current owner
                        requestedEntity.OwnerShipWasTransfered = true;
                        requestedEntity.ownerId = newPlayer.Id;

                        if (PeerClient.logLevel >= LogLevel.Info)
                        {
                            Debug.LogWarning(requestedEntity + " ownership transfered to: " + newPlayer.Id);
                        }
                        Delegate.OnOwnership(requestedEntity, oldPlayer);
                    }
                    break;
                case OwnershipOption.Request:
                    if (oldPlayer == Room.localPlayer || Room.isMasterClient)
                    {
                        if ((requestedEntity.ownerId == SceneConnect.player.Id) || (Room.isMasterClient && !requestedEntity.isOwnerActive))
                        {
                            Delegate.OnOwnership(requestedEntity, oldPlayer);
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Executes a received RPC event
        /// </summary>
        protected internal void OnRpc(int senderID, Hashtable rpcData)
        {
            if (rpcData == null || !rpcData.ContainsKey((byte)0))
            {
                return;
            }

            // ts: updated with "flat" event data
            int netEntityID = (int)rpcData[(byte)0]; // LIMITS PHOTONVIEWS&PLAYERS
            int otherSidePrefix = 0;    // by default, the prefix is 0 (and this is not being sent)
            if (rpcData.ContainsKey((byte)1))
            {
                otherSidePrefix = (short)rpcData[(byte)1];
            }


            string inMethodName = (string)rpcData[(byte)3];

            object[] inMethodParameters = null;
            if (rpcData.ContainsKey((byte)4))
            {
                inMethodParameters = (object[])rpcData[(byte)4];
            }

            if (inMethodParameters == null)
            {
                inMethodParameters = new object[0];
            }

            NetworkEntity netEntity = GetEntity(netEntityID);
            if (netEntity == null)
            {
                int viewOwnerId = netEntityID / MAX_ENTITY_IDS;
                bool owningPv = (viewOwnerId == Room.localPlayer.Id);
                bool ownerSent = (viewOwnerId == senderID);

                if (owningPv)
                {
                    Debug.LogWarning("Received RPC \"" + inMethodName + "\" for netEntityID " + netEntityID + " but this NetworkEntity does not exist! View was/is ours." + (ownerSent ? " Owner called." : " Remote called.") + " By: " + senderID);
                }
                else
                {
                    Debug.LogWarning("Received RPC \"" + inMethodName + "\" for netEntityID " + netEntityID + " but this NetworkEntity does not exist! Was remote PV." + (ownerSent ? " Owner called." : " Remote called.") + " By: " + senderID + " Maybe GO was destroyed but RPC not cleaned up.");
                }
                return;
            }

            if (netEntity.prefix != otherSidePrefix)
            {
                Debug.LogError("Received RPC \"" + inMethodName + "\" on viewID " + netEntityID + " with a prefix of " + otherSidePrefix + ", our prefix is " + netEntity.prefix + ". The RPC has been ignored.");
                return;
            }

            // Get method name
            if (string.IsNullOrEmpty(inMethodName))
            {
                Debug.LogError("Malformed RPC; this should never occur. Content: " + rpcData);
                return;
            }

            if (PeerClient.logLevel >= LogLevel.Debug)
                Debug.Log("Received RPC: " + inMethodName);


            // SetReceiving filtering
            if (netEntity.group != 0 && !allowedReceivingGroups.Contains(netEntity.group))
            {
                return; // Ignore group
            }

            Type[] argTypes = new Type[0];
            if (inMethodParameters.Length > 0)
            {
                argTypes = new Type[inMethodParameters.Length];
                int i = 0;
                for (int index = 0; index < inMethodParameters.Length; index++)
                {
                    object objX = inMethodParameters[index];
                    if (objX == null)
                    {
                        argTypes[i] = null;
                    }
                    else
                    {
                        argTypes[i] = objX.GetType();
                    }

                    i++;
                }
            }

            int receivers = 0;
            int foundMethods = 0;
            if (netEntity.RpcMonoBehaviours == null || netEntity.RpcMonoBehaviours.Length == 0)
            {
                netEntity.RefreshRpcMonoBehaviourCache();
            }

            for (int componentsIndex = 0; componentsIndex < netEntity.RpcMonoBehaviours.Length; componentsIndex++)
            {
                MonoBehaviour monob = netEntity.RpcMonoBehaviours[componentsIndex];
                if (monob == null)
                {
                    Debug.LogError("ERROR You have missing MonoBehaviours on your gameobjects!");
                    continue;
                }

                Type type = monob.GetType();

                // Get [PunRPC] methods from cache
                List<MethodInfo> cachedRPCMethods = null;
                bool methodsOfTypeInCache = this.monoRPCMethodsCache.TryGetValue(type, out cachedRPCMethods);

                if (!methodsOfTypeInCache)
                {
                    var methodsAll = type.GetMethods();
                    var entries = new List<MethodInfo>();
                    for (var i = 0; i < methodsAll.Length; i++)
                    {
                        var method = methodsAll[i];
                        if (method is GameRPC)
                        {
                            entries.Add(method);
                        }
                    }

                    this.monoRPCMethodsCache[type] = entries;
                    cachedRPCMethods = entries;
                }

                if (cachedRPCMethods == null)
                {
                    continue;
                }

                // Check cache for valid methodname+arguments
                for (int index = 0; index < cachedRPCMethods.Count; index++)
                {
                    MethodInfo mInfo = cachedRPCMethods[index];
                    if (!mInfo.Name.Equals(inMethodName)) continue;
                    foundMethods++;
                    ParameterInfo[] pArray = mInfo.GetCachedParemeters();

                    if (pArray.Length == argTypes.Length)
                    {
                        // Normal, PhotonNetworkMessage left out
                        if (!ReflectClass.CheckTypeMatch(pArray, argTypes)) continue;
                        receivers++;
                        object result = mInfo.Invoke((object)monob, inMethodParameters);
                        if (mInfo.ReturnType == typeof(IEnumerator))
                        {
                            monob.StartCoroutine((IEnumerator)result);
                        }
                    }
                    else if ((pArray.Length - 1) == argTypes.Length)
                    {
                        // Check for PhotonNetworkMessage being the last
                        if (!ReflectClass.CheckTypeMatch(pArray, argTypes)) continue;
                        if (pArray[pArray.Length - 1].ParameterType != typeof(MessageInfo)) continue;
                        receivers++;

                        int sendTime = (int)rpcData[(byte)2];
                        object[] deParamsWithInfo = new object[inMethodParameters.Length + 1];
                        inMethodParameters.CopyTo(deParamsWithInfo, 0);


                        deParamsWithInfo[deParamsWithInfo.Length - 1] = new MessageInfo(Room.GetPlayerWithId(senderID), netEntity);

                        object result = mInfo.Invoke((object)monob, deParamsWithInfo);
                        if (mInfo.ReturnType == typeof(IEnumerator))
                        {
                            monob.StartCoroutine((IEnumerator)result);
                        }
                    }
                    else if (pArray.Length == 1 && pArray[0].ParameterType.IsArray)
                    {
                        receivers++;
                        object result = mInfo.Invoke((object)monob, new object[] { inMethodParameters });
                        if (mInfo.ReturnType == typeof(IEnumerator))
                        {
                            monob.StartCoroutine((IEnumerator)result);
                        }
                    }
                }
            }

            // Error handling
            if (receivers != 1)
            {
                string argsString = string.Empty;
                for (int index = 0; index < argTypes.Length; index++)
                {
                    Type ty = argTypes[index];
                    if (argsString != string.Empty)
                    {
                        argsString += ", ";
                    }

                    if (ty == null)
                    {
                        argsString += "null";
                    }
                    else
                    {
                        argsString += ty.Name;
                    }
                }

                if (receivers == 0)
                {
                    if (foundMethods == 0)
                    {
                        Debug.LogError("NetworkEntity with ID " + netEntityID + " has no method \"" + inMethodName + "\" marked with the [PunRPC](C#) or @PunRPC(JS) property! Args: " + argsString);
                    }
                    else
                    {
                        Debug.LogError("NetworkEntity with ID " + netEntityID + " has no method \"" + inMethodName + "\" that takes " + argTypes.Length + " argument(s): " + argsString);
                    }
                }
                else
                {
                    Debug.LogError("NetworkEntity with ID " + netEntityID + " has " + receivers + " methods \"" + inMethodName + "\" that takes " + argTypes.Length + " argument(s): " + argsString + ". Should be just one?");
                }
            }
        }

        internal GameObject OnInstance(int playerId, Hashtable evData, GameObject resourceGameObject)
        {
            //var player = Room.GetPlayerWithId(playerId);
            // some values always present:
            string prefabName = (string)evData[(byte)0];
            //int serverTime = (int)evData[(byte)6];
            int instantiationId = (int)evData[(byte)6];

            Vector3 position;
            if (evData.ContainsKey((byte)1))
            {
                position = (Vector3)evData[(byte)1];
            }
            else
            {
                position = Vector3.zero;
            }

            Quaternion rotation = Quaternion.identity;
            if (evData.ContainsKey((byte)2))
            {
                rotation = (Quaternion)evData[(byte)2];
            }

            byte group = 0;
            if (evData.ContainsKey((byte)3))
            {
                group = (byte)evData[(byte)3];
            }

            short objLevelPrefix = 0;
            if (evData.ContainsKey((byte)7))
            {
                objLevelPrefix = (short)evData[(byte)7];
            }

            int[] viewsIDs;
            if (evData.ContainsKey((byte)4))
            {
                viewsIDs = (int[])evData[(byte)4];
            }
            else
            {
                viewsIDs = new int[1] { instantiationId };
            }

            object[] incomingInstantiationData;
            if (evData.ContainsKey((byte)5))
            {
                incomingInstantiationData = (object[])evData[(byte)5];
            }
            else
            {
                incomingInstantiationData = null;
            }

            // SetReceiving filtering
            if (group != 0 && !allowedReceivingGroups.Contains(group))
            {
                return null; // Ignore group
            }

            // load prefab, if it wasn't loaded before (calling methods might do this)
            if (resourceGameObject == null)
            {
                Room.OnInstance(playerId);
                //if (!UsePrefabCache || !PrefabCache.TryGetValue(prefabName, out resourceGameObject))
                //{
                //    resourceGameObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
                //    if (UsePrefabCache)
                //    {
                //        PrefabCache.Add(prefabName, resourceGameObject);
                //    }
                //}
                resourceGameObject = Delegate.OnInstance(prefabName);

                if (resourceGameObject == null)
                {
                    Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + prefabName + "]. Please verify you have this gameobject in a Resources folder.");
                    return null;
                }
            }

            // now modify the loaded "blueprint" object before it becomes a part of the scene (by instantiating it)
            NetworkEntity[] resourcePVs = resourceGameObject.GetEntitysInChildren();
            if (resourcePVs.Length != viewsIDs.Length)
            {
                throw new Exception("Error in Instantiation! The resource's PhotonView count is not the same as in incoming data.");
            }

            for (int i = 0; i < viewsIDs.Length; i++)
            {
                // NOTE instantiating the loaded resource will keep the viewID but would not copy instantiation data, so it's set below
                // so we only set the viewID and instantiationId now. the instantiationData can be fetched
                resourcePVs[i].entityId = viewsIDs[i];
                resourcePVs[i].prefix = objLevelPrefix;
                resourcePVs[i].instantiationId = instantiationId;
                resourcePVs[i].isRuntimeInstantiated = true;
            }

            this.StoreInstantiationData(instantiationId, incomingInstantiationData);

            // load the resource and set it's values before instantiating it:
            GameObject go = (GameObject)GameObject.Instantiate(resourceGameObject, position, rotation);

            for (int i = 0; i < viewsIDs.Length; i++)
            {
                // NOTE instantiating the loaded resource will keep the viewID but would not copy instantiation data, so it's set below
                // so we only set the viewID and instantiationId now. the instantiationData can be fetched
                resourcePVs[i].entityId = 0;
                resourcePVs[i].prefix = -1;
                resourcePVs[i].prefixBackup = -1;
                resourcePVs[i].instantiationId = -1;
                resourcePVs[i].isRuntimeInstantiated = false;
            }

            this.RemoveInstantiationData(instantiationId);

            return go;
        }

        private void OnDestory(int t, int id)
        {
            NetworkEntity pvToDestroy = null;
            if (id == -1)
            {
                listener.DestroyAll(true);
            }
            else if (t == 1)
            {
                if (entityList.TryGetValue(id, out pvToDestroy))
                {
                    RemoveInstantiatedGO(pvToDestroy.gameObject, true);
                }
                else
                {
                    if (PeerClient.logLevel >= LogLevel.Error) Debug.LogError("Ev Destroy Failed. Could not find Entity with instantiationId " + id);
                }
            }
            else if (t == 2)
            {
                listener.DestroyPlayerObjects(id, true);
            }
        }
        #endregion

        private static Dictionary<int, object[]> tempInstantiationData = new Dictionary<int, object[]>();

        private void StoreInstantiationData(int instantiationId, object[] instantiationData)
        {
            tempInstantiationData[instantiationId] = instantiationData;
        }

        public static object[] FetchInstantiationData(int instantiationId)
        {
            object[] data = null;
            if (instantiationId == 0)
            {
                return null;
            }

            tempInstantiationData.TryGetValue(instantiationId, out data);
            return data;
        }

        private void RemoveInstantiationData(int instantiationId)
        {
            tempInstantiationData.Remove(instantiationId);
        }

        public void DestroyPlayerObjects(int playerId, bool localOnly)
        {
            if (playerId <= 0)
            {
                Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
                return;
            }

            if (!localOnly)
            {
                EmitDestroyOfPlayer(playerId);
            }

            // locally cleaning up that player's objects
            HashSet<GameObject> playersGameObjects = new HashSet<GameObject>();
            foreach (NetworkEntity entity in entityList.Values)
            {
                if (entity != null && entity.CreatorActorNr == playerId)
                {
                    playersGameObjects.Add(entity.gameObject);
                }
            }

            // any non-local work is already done, so with the list of that player's objects, we can clean up (locally only)
            foreach (GameObject gameObject in playersGameObjects)
            {
                RemoveInstantiatedGO(gameObject, true);
            }

            // with ownership transfer, some objects might lose their owner.
            // in that case, the creator becomes the owner again. every client can apply this. done below.
            foreach (NetworkEntity entity in entityList.Values)
            {
                if (entity.ownerId == playerId)
                {
                    entity.ownerId = entity.CreatorActorNr;
                }
            }
        }

        protected void DestroyAll(bool localOnly)
        {
            if (tempInstantiationData.Count > 0)
            {
                Debug.LogWarning("It seems some instantiation is not completed, as instantiation data is used. You should make sure instantiations are paused when calling this method. Cleaning now, despite this.");
            }

            HashSet<GameObject> instantiatedGos = new HashSet<GameObject>();
            foreach (NetworkEntity entity in entityList.Values)
            {
                if (entity.isRuntimeInstantiated)
                {
                    instantiatedGos.Add(entity.gameObject); // HashSet keeps each object only once
                }
            }

            foreach (GameObject go in instantiatedGos)
            {
                RemoveInstantiatedGO(go, true);
            }

            tempInstantiationData.Clear(); // should be empty but to be safe we clear (no new list needed)
            lastUsedViewSubId = 0;
            lastUsedViewSubIdScene = 0;
        }


        protected internal void RemoveInstantiatedGO(GameObject go, bool localOnly)
        {
            if (go == null)
            {
                Debug.LogError("Failed to 'network-remove' GameObject because it's null.");
                return;
            }

            NetworkEntity[] entitys = go.GetComponentsInChildren<NetworkEntity>(true);
            if (entitys == null || entitys.Length <= 0)
            {
                Debug.LogError("Failed to 'network-remove' GameObject because has no PhotonView components: " + go);
                return;
            }

            NetworkEntity viewZero = entitys[0];
            int creatorId = viewZero.CreatorActorNr;            // creatorId of obj is needed to delete EvInstantiate (only if it's from that user)
            int instantiationId = viewZero.instantiationId;     // actual, live InstantiationIds start with 1 and go up

            // Don't remove GOs that are owned by others (unless this is the master and the remote player left)
            if (!localOnly)
            {
                if (!viewZero.isMine)
                {
                    Debug.LogError("Failed to 'network-remove' GameObject. Client is neither owner nor masterClient taking over for owner who left: " + viewZero);
                    return;
                }

                // Don't remove the Instantiation from the server, if it doesn't have a proper ID
                if (instantiationId < 1)
                {
                    Debug.LogError("Failed to 'network-remove' GameObject because it is missing a valid InstantiationId on view: " + viewZero + ". Not Destroying GameObject or PhotonViews!");
                    return;
                }
            }

            // cleanup instantiation (event and local list)
            if (!localOnly)
            {
                EmitDestroyOfInstantiate(instantiationId, creatorId);
            }

            // cleanup PhotonViews and their RPCs events (if not localOnly)
            for (int j = entitys.Length - 1; j >= 0; j--)
            {
                NetworkEntity entity = entitys[j];
                if (entity == null)
                {
                    continue;
                }

                // we only destroy/clean PhotonViews that were created by PhotonNetwork.Instantiate (and those have an instantiationId!)
                if (entity.instantiationId >= 1)
                {
                    LocalCleanEntity(entity);
                }
            }

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Network destroy Instantiated GO: " + go.name);
            }

            GameObject.Destroy(go);
        }

        public static bool LocalCleanEntity(NetworkEntity entity)
        {
            entity.removedFromLocalList = true;
            return entityList.Remove(entity.entityId);
        }

        public static NetworkEntity GetEntity(int entityId)
        {
            NetworkEntity result = null;
            entityList.TryGetValue(entityId, out result);

            if (result == null)
            {
                NetworkEntity[] entitys = GameObject.FindObjectsOfType(typeof(NetworkEntity)) as NetworkEntity[];

                for (int i = 0; i < entitys.Length; i++)
                {
                    NetworkEntity entity = entitys[i];
                    if (entity.entityId == entityId)
                    {
                        if (entity.didAwake)
                        {
                            Debug.LogWarning("Had to lookup view that wasn't in entityList: " + entity);
                        }
                        return entity;
                    }
                }
            }

            return result;
        }

        /// RPC Hashtable Structure
        /// (byte)0 -> (int) ViewId (combined from actorNr and actor-unique-id)
        /// (byte)1 -> (short) prefix (level)
        /// (byte)2 -> (int) server timestamp
        /// (byte)3 -> (string) methodname
        /// (byte)4 -> (object[]) parameters
        /// (byte)5 -> (byte) method shortcut (alternative to name)
        ///
        /// This is sent as event (code: 200) which will contain a sender (origin of this RPC).

        internal void EmitRPC(NetworkEntity entity, string methodName, SyncTargets target, NetworkPlayer player, params object[] parameters)
        {
            if (blockSendingGroups.Contains(entity.group))
            {
                return; // Block sending on this group
            }

            if (entity.entityId < 1)
            {
                Debug.LogError("Illegal entity ID:" + entity.entityId + " method: " + methodName + " GO:" + entity.gameObject.name);
            }

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Sending RPC \"" + methodName + "\" to target: " + target + " or player:" + player + ".");
            }


            //ts: changed RPCs to a one-level hashtable as described in internal.txt
            Hashtable rpcEvent = new Hashtable();
            rpcEvent[(byte)0] = (int)entity.entityId; // LIMITS NETWORKVIEWS&PLAYERS
            if (entity.prefix > 0)
            {
                rpcEvent[(byte)1] = (short)entity.prefix;
            }


            // send name or shortcut (if available)
            int shortcut = 0;
            if (rpcShortcuts.TryGetValue(methodName, out shortcut))
            {
                rpcEvent[(byte)4] = (byte)shortcut; // LIMITS RPC COUNT
            }
            else
            {
                rpcEvent[(byte)2] = methodName;
            }

            if (parameters != null && parameters.Length > 0)
            {
                rpcEvent[(byte)3] = (object[])parameters;
            }


            // if sent to target player, this overrides the target
            if (player != null)
            {
                if (Room.localPlayer.Id == player.Id)
                {
                    this.OnRpc(Room.localPlayer.Id, rpcEvent);
                }
                else
                {
                    //RaiseEventOptions options = new RaiseEventOptions() { TargetActors = new int[] { player.Id } };
                    n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, player.Id, entity.group, false });
                }

                return;
            }

            // send to a specific set of players
            if (target == SyncTargets.All)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, false });

                // Execute local
                this.OnRpc(Room.localPlayer.Id, rpcEvent);
            }
            else if (target == SyncTargets.Others)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, false });
            }
            else if (target == SyncTargets.AllBuffered)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, true });

                // Execute local
                this.OnRpc(Room.localPlayer.Id, rpcEvent);
            }
            else if (target == SyncTargets.OthersBuffered)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, true });
            }
            else if (target == SyncTargets.MasterClient)
            {
                if (Room.isMasterClient)
                {
                    this.OnRpc(Room.localPlayer.Id, rpcEvent);
                }
                else
                {
                    //RaiseEventOptions options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
                    n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, Room.masterClient.Id, entity.group, false });
                }
            }
            else if (target == SyncTargets.AllViaServer)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group, Receivers = ReceiverGroup.All };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, false });
                if (PeerClient.offlineMode)
                {
                    this.OnRpc(Room.localPlayer.Id, rpcEvent);
                }
            }
            else if (target == SyncTargets.AllBufferedViaServer)
            {
                //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group, Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
                n.Emit(GameEvent.RPC, new object[] { Room.localPlayer.Id, rpcEvent, 0, entity.group, true });
                if (PeerClient.offlineMode)
                {
                    this.OnRpc(Room.localPlayer.Id, rpcEvent);
                }
            }
            else
            {
                Debug.LogError("Unsupported target enum: " + target);
            }
        }

        public static int ObjectsInOneUpdate = 10;

        #region sync entity
        internal static void Update()
        {
            listener.UpdateEntity();
        }

        private void UpdateEntity()
        {
            if (!PeerClient.connected || PeerClient.offlineMode || Room.mActors == null)
            {
                return;
            }

            // no need to send OnSerialize messages while being alone (these are not buffered anyway)
            if (Room.mActors.Count <= 1)
            {
#if !PHOTON_DEVELOP
                return;
#endif
            }


            /* Format of the data hashtable:
             * Hasthable dataPergroup*
             *  [(byte)0] = currentLevelPrefix;  OPTIONAL!
             *
             *  [(byte)10] = data 1
             *  [(byte)11] = data 2 ...
             *
             *  We only combine updates for XY objects into one RaiseEvent to avoid fragmentation
             */

            int countOfUpdatesToSend = 0;

            // reset cached raisedEventOptions;
            // we got updates to send. every group is send it's own message and unreliable and reliable are split as well
            //options.Reset();

#if PHOTON_DEVELOP
        options.Receivers = ReceiverGroup.All;
#endif

            List<int> toRemove = null;

            var enumerator = entityList.GetEnumerator();   // replacing foreach (PhotonView view in this.photonViewList.Values) for memory allocation improvement
            while (enumerator.MoveNext())
            {
                NetworkEntity entity = enumerator.Current.Value;

                if (entity == null)
                {
                    Debug.LogError(string.Format("NetworkEntity with ID {0} wasn't properly unregistered! Please report this case to developer@photonengine.com", enumerator.Current.Key));

                    if (toRemove == null)
                    {
                        toRemove = new List<int>(4);
                    }
                    toRemove.Add(enumerator.Current.Key);

                    continue;
                }

                // a client only sends updates for active, synchronized PhotonViews that are under it's control (isMine)
                if (entity.synchronization == EntitySynchronization.Off || entity.isMine == false || entity.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                if (blockSendingGroups.Contains(entity.group))
                {
                    continue; // Block sending on this group
                }

                object[] evData = this.OnSerializeWrite(entity);
                if (evData == null)
                {
                    continue;
                }

                if (entity.synchronization == EntitySynchronization.ReliableDeltaCompressed || entity.mixedModeIsReliable)
                {
                    Hashtable groupHashtable = null;
                    bool found = this.dataPerGroupReliable.TryGetValue(entity.group, out groupHashtable);
                    if (!found)
                    {
                        groupHashtable = new Hashtable(ObjectsInOneUpdate);
                        this.dataPerGroupReliable[entity.group] = groupHashtable;
                    }

                    groupHashtable.Add((byte)(groupHashtable.Count + 10), evData);
                    countOfUpdatesToSend++;

                    // if any group has XY elements, we should send it right away (to avoid bigger messages which need fragmentation and reliable transfer).
                    if (groupHashtable.Count >= ObjectsInOneUpdate)
                    {
                        countOfUpdatesToSend -= groupHashtable.Count;

                        //options.InterestGroup = (byte)entity.group;
                        //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                        if (currentLevelPrefix >= 0)
                        {
                            groupHashtable[(byte)0] = currentLevelPrefix;
                        }

                        n.Emit(GameEvent.Serialize, new object[] { Room.localPlayer.Id, groupHashtable, entity.group, true });
                        //Debug.Log("SendSerializeReliable (10) " + PhotonNetwork.networkingPeer.ByteCountLastOperation);
                        groupHashtable.Clear();
                    }
                }
                else
                {
                    Hashtable groupHashtable = null;
                    bool found = this.dataPerGroupUnreliable.TryGetValue(entity.group, out groupHashtable);
                    if (!found)
                    {
                        groupHashtable = new Hashtable(ObjectsInOneUpdate);
                        this.dataPerGroupUnreliable[entity.group] = groupHashtable;
                    }

                    groupHashtable.Add((byte)(groupHashtable.Count + 10), evData);
                    countOfUpdatesToSend++;

                    // if any group has XY elements, we should send it right away (to avoid bigger messages which need fragmentation and reliable transfer).
                    if (groupHashtable.Count >= ObjectsInOneUpdate)
                    {
                        countOfUpdatesToSend -= groupHashtable.Count;

                        //options.InterestGroup = (byte)entity.group;
                        //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                        if (currentLevelPrefix >= 0)
                        {
                            groupHashtable[(byte)0] = currentLevelPrefix;
                        }

                        n.Emit(GameEvent.Serialize, new object[] { Room.localPlayer.Id, groupHashtable, entity.group, false });
                        groupHashtable.Clear();
                    }
                }
            }   // all views serialized

            if (toRemove != null)
            {
                for (int idx = 0, count = toRemove.Count; idx < count; ++idx)
                {
                    entityList.Remove(toRemove[idx]);
                }
            }

            // if we didn't produce anything to send, don't do it
            if (countOfUpdatesToSend == 0)
            {
                return;
            }


            foreach (int groupId in this.dataPerGroupReliable.Keys)
            {
                //options.InterestGroup = (byte)groupId;
                Hashtable groupHashtable = this.dataPerGroupReliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }

                //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                if (currentLevelPrefix >= 0)
                {
                    groupHashtable[(byte)0] = currentLevelPrefix;
                }

                n.Emit(GameEvent.Serialize, new object[] { Room.localPlayer.Id, groupHashtable, groupId, true });
                groupHashtable.Clear();
            }
            foreach (int groupId in this.dataPerGroupUnreliable.Keys)
            {
                //options.InterestGroup = (byte)groupId;
                Hashtable groupHashtable = this.dataPerGroupUnreliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }

                //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                if (currentLevelPrefix >= 0)
                {
                    groupHashtable[(byte)0] = currentLevelPrefix;
                }

                n.Emit(GameEvent.Serialize, new object[] { Room.localPlayer.Id, groupHashtable, groupId, false });
                groupHashtable.Clear();
            }
        }

        private object[] OnSerializeWrite(NetworkEntity entity)
        {
            if (entity.synchronization == EntitySynchronization.Off)
            {
                return null;
            }


            // each view creates a list of values that should be sent
            MessageInfo info = new MessageInfo(SceneConnect.player, entity);
            this.pStream.ResetWriteStream();
            this.pStream.SendNext(null);
            this.pStream.SendNext(null);
            this.pStream.SendNext(null);
            entity.Serialize(this.pStream, info);

            // check if there are actual values to be sent (after the "header" of viewId, (bool)compressed and (int[])nullValues)
            if (this.pStream.Count <= SyncFirstValue)
            {
                return null;
            }


            object[] currentValues = this.pStream.ToArray();
            currentValues[0] = entity.entityId;
            currentValues[1] = false;
            currentValues[2] = null;

            if (entity.synchronization == EntitySynchronization.Unreliable)
            {
                return currentValues;
            }


            // ViewSynchronization: Off, Unreliable, UnreliableOnChange, ReliableDeltaCompressed
            if (entity.synchronization == EntitySynchronization.UnreliableOnChange)
            {
                if (AlmostEquals(currentValues, entity.lastOnSerializeDataSent))
                {
                    if (entity.mixedModeIsReliable)
                    {
                        return null;
                    }

                    entity.mixedModeIsReliable = true;
                    entity.lastOnSerializeDataSent = currentValues;
                }
                else
                {
                    entity.mixedModeIsReliable = false;
                    entity.lastOnSerializeDataSent = currentValues;
                }

                return currentValues;
            }

            if (entity.synchronization == EntitySynchronization.ReliableDeltaCompressed)
            {
                object[] dataToSend = DeltaCompressionWrite(entity.lastOnSerializeDataSent, currentValues);

                entity.lastOnSerializeDataSent = currentValues;

                return dataToSend;
            }

            return null;
        }
        private void OnSerialize(int actorId, Hashtable serializeData)
        {
            var originatingPlayer = Room.GetPlayerWithId(actorId);
            short remoteLevelPrefix = -1;
            byte initialDataIndex = 10;
            int headerLength = 1;
            if (serializeData.ContainsKey((byte)0))
            {
                remoteLevelPrefix = (short)serializeData[(byte)0];
                headerLength = 2;
            }

            for (byte s = initialDataIndex; s - initialDataIndex < serializeData.Count - headerLength; s++)
            {
                listener.OnSerializeRead(serializeData[s] as object[], originatingPlayer, remoteLevelPrefix);
            }
        }

        private void OnSerializeRead(object[] data, NetworkPlayer sender, short correctPrefix)
        {
            // read view ID from key (byte)0: a int-array (PUN 1.17++)
            int entityId = (int)data[SyncViewId];


            // debug:
            //LogObjectArray(data);

            NetworkEntity entity = GetEntity(entityId);
            if (entity == null)
            {
                Debug.LogWarning("Received OnSerialization for view ID " + entity + ". We have no such NetworkEntity! Ignored this if you're leaving a room. State: " + PeerClient.client.State);
                return;
            }

            if (entity.prefix > 0 && correctPrefix != entity.prefix)
            {
                Debug.LogError("Received OnSerialization for view ID " + entity + " with prefix " + correctPrefix + ". Our prefix is " + entity.prefix);
                return;
            }

            // SetReceiving filtering
            if (entity.group != 0 && !allowedReceivingGroups.Contains(entity.group))
            {
                return; // Ignore group
            }

            if (entity.synchronization == EntitySynchronization.ReliableDeltaCompressed)
            {
                object[] uncompressed = this.DeltaCompressionRead(entity.lastOnSerializeDataReceived, data);
                //LogObjectArray(uncompressed,"uncompressed ");
                if (uncompressed == null)
                {
                    // Skip this packet as we haven't got received complete-copy of this view yet.
                    if (PeerClient.logLevel >= LogLevel.Info)
                    {
                        Debug.Log("Skipping packet for " + entity.name + " [" + entity.entityId + "] as we haven't received a full packet for delta compression yet. This is OK if it happens for the first few frames after joining a game.");
                    }
                    return;
                }

                // store last received values (uncompressed) for delta-compression usage
                entity.lastOnSerializeDataReceived = uncompressed;
                data = uncompressed;
            }

            // This is when joining late to assign ownership to the sender
            // this has nothing to do with reading the actual synchronization update.
            // We don't do anything is OwnerShip Was Touched, which means we got the infos already. We only possibly act if ownership was never transfered.
            // We do override OwnerShipWasTransfered if owner is the masterClient.
            if (sender.Id != entity.ownerId && (!entity.OwnerShipWasTransfered || entity.ownerId == 0))
            {
                // obviously the owner changed and we didn't yet notice.
                //Debug.Log("Adjusting owner to sender of updates. From: " + view.ownerId + " to: " + sender.ID);
                entity.ownerId = sender.Id;
            }

            this.readStream.SetReadStream(data, 3);
            MessageInfo info = new MessageInfo(sender, entity);

            entity.Deserialize(this.readStream, info);
        }


        public const int SyncViewId = 0;
        public const int SyncCompressed = 1;
        public const int SyncNullValues = 2;
        public const int SyncFirstValue = 3;

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
        #endregion

        #region scene
        internal protected void LoadLevel()
        {
            Hashtable setScene = new Hashtable();
            setScene[CurrentSceneProperty] = (string)SceneManagerHelper.ActiveSceneName;

            Room.SetCustomProperties(setScene);
            currentLevelPrefix = short.Parse(SceneManagerHelper.ActiveSceneBuildIndex.ToString());
        }
        #endregion

        private void ClearRoom()
        {
            Room.Clear();
            Room = null;
            allowedReceivingGroups = new HashSet<byte>();
            blockSendingGroups = new HashSet<byte>();
            List<int> removeKeys = new List<int>();
            foreach (KeyValuePair<int, NetworkEntity> kvp in entityList)
            {
                NetworkEntity entity = kvp.Value;
                if (entity == null)
                {
                    removeKeys.Add(kvp.Key);
                }
            }

            for (int index = 0; index < removeKeys.Count; index++)
            {
                int key = removeKeys[index];
                entityList.Remove(key);
            }
            Delegate.OnLeaveGame();
        }

        #region Allocate
        internal static int AllocateEntityId()
        {
            int manualId = AllocateEntityId(SceneConnect.player.Id);
            manuallyAllocatedEntityIds.Add(manualId);
            return manualId;
        }

        internal static int AllocateSceneEntityId()
        {
            if (!isMasterClient)
            {
                Debug.LogError("Only the Master Client can AllocateSceneEntityId(). Check isMasterClient!");
                return -1;
            }

            int manualId = AllocateEntityId(0);
            manuallyAllocatedEntityIds.Add(manualId);
            return manualId;
        }

        internal static int AllocateEntityId(int ownerId)
        {
            int newSubId = ownerId > 0 ? lastUsedViewSubId : lastUsedViewSubIdScene;
            int newEntityId;
            int ownerIdOffset = ownerId * MAX_ENTITY_IDS;
            for (int i = newSubId; i < MAX_ENTITY_IDS; i++)
            {
                newSubId = (i + 1) % MAX_ENTITY_IDS;
                if (newSubId == 0)
                {
                    continue;   // avoid using subID 0
                }

                newEntityId = newSubId + ownerIdOffset;
                if (entityList.ContainsKey(newEntityId)){
                    continue;
                }
                if (manuallyAllocatedEntityIds.Contains(newEntityId)){
                    continue;
                }
                if(ownerId > 0){
                    lastUsedViewSubId = newSubId;
                }else{
                    lastUsedViewSubIdScene = newSubId;
                }
                return newEntityId;
            }

            throw new Exception(string.Format("AllocateEntityId() failed. User {0} is out of subIds, as all Entity are used.", ownerId));
        }

        internal static int[] AllocateSceneEntityIds(int countOfNewEntitys)
        {
            int[] entityIds = new int[countOfNewEntitys];
            for (int entity = 0; entity < countOfNewEntitys; entity++)
            {
                entityIds[entity] = AllocateEntityId(0);
            }

            return entityIds;
        }

        internal static void UnAllocateEntityId(int entityId)
        {
            manuallyAllocatedEntityIds.Remove(entityId);

            if (entityList.ContainsKey(entityId))
            {
                Debug.LogWarning(string.Format("UnAllocateEntityID() should be called after the Entity was destroyed (GameObject.Destroy()). entityId: {0} still found in: {1}", entityId, entityList[entityId]));
            }
        }
        #endregion

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
#if UNITY_EDITOR

        public static void InternalCleanPhotonMonoFromSceneIfStuck()
        {
            Handler[] Handlers = GameObject.FindObjectsOfType(typeof(Handler)) as Handler[];
            if (Handlers != null && Handlers.Length > 0)
            {
                foreach (Handler Handler in Handlers)
                {
                    Handler.gameObject.hideFlags = 0;

                    if (Handler.gameObject != null && Handler.gameObject.name == "GalaxyBack")
                    {
                        GameObject.DestroyImmediate(Handler.gameObject);
                    }

                    Component.DestroyImmediate(Handler);
                }
            }
        }
#endif
    }
}
