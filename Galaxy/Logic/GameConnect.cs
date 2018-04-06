using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;
using System.Reflection;
using Gj.Galaxy.Utils;
using MessagePack;

namespace Gj.Galaxy.Logic
{
    //todo 离线模式
    internal class GameEvent
    {
        public const byte Members = 0;
        public const byte Ready = 1;
        public const byte OwnershipTakeover = 2;
        public const byte OwnershipGiveout = 3;
        public const byte Exit = 4;
        public const byte ChangeGroups = 5;
        public const byte Sync = 6;

        public const byte ReadyAll = 254;
        public const byte AssignPlayer = 253;
        public const byte SwitchMaster = 252;
        public const byte Finish = 251;
        public const byte Join = 250;
        public const byte Leave = 249;
        public const byte Ownership = 248;
    }
    internal class SyncEvent
    {
        public const byte Init = 0;
        public const byte Command = 2;
        public const byte Instance = 3;
        public const byte Destory = 4;
        public const byte Serialize = 5;
        public const byte ChangeRoom = 6;
        public const byte ChangePlayer = 7;
    }

    internal enum GameStage
    {
        None,
        Connect,
        Join,
        Ready,
        ReadyAll,
        Init,
        Start,
        Finish
    }

    public enum InstanceRelation
    {
        Player,
        Scene,
        OtherPlayer
    }
    public interface GameListener
    {
        void OnFinish(bool exit, Dictionary<string, object> result);
        void OnStart();
        void OnLeaveGame();
        void OnOwnership(NetworkEntity entity, GamePlayer oldPlayer);
        GameObject OnInstance(string prefabName, GamePlayer player, object data);
        void OnCommand(NetworkEntity entity, GamePlayer player, string type, string category, float value);
        void OnDestroyInstance(GameObject gameObject, GamePlayer player);
    }

    public interface GameRoomListener
    {
        void OnEnter();
        void OnFail(string reason);
        void OnRoomChange(Dictionary<string, object> props);
        void OnPlayerJoin(GamePlayer player);
        void OnPlayerLeave(GamePlayer player);
        void OnPlayerRejoin(GamePlayer player);
        void OnPlayerChange(GamePlayer player, Dictionary<string, object> props);
        void OnReadyPlayer(GamePlayer player);
        void OnReadyAll();
    }

    public class GameDelegate : GameListener
    {
        public delegate void OnFinishDelegate(bool exit, Dictionary<string, object> result);
        public delegate void OnStartDelegate();
        public delegate void OnLeaveGameDelegate();
        public delegate void OnOwnershipDelegate(NetworkEntity entity, GamePlayer oldPlayer);
        public delegate void OnCommandDelegate(NetworkEntity entity, GamePlayer player, string type, string category, float value);
        public delegate GameObject OnInstanceDelegate(string prefabName, GamePlayer player, object data);
        public delegate void OnDestroyInstanceDelegate(GameObject gameObject, GamePlayer player);

        public OnFinishDelegate OnFinish;
        public OnStartDelegate OnStart;
        public OnLeaveGameDelegate OnLeaveGame;
        public OnOwnershipDelegate OnOwnership;
        public OnCommandDelegate OnCommand;
        public OnInstanceDelegate OnInstance;
        public OnDestroyInstanceDelegate OnDestroyInstance;

        void GameListener.OnFinish(bool exit, Dictionary<string, object> result)
        {
            if (OnFinish != null) OnFinish(exit, result);
        }

        void GameListener.OnStart()
        {
            if (OnStart != null) OnStart();
        }

        void GameListener.OnLeaveGame()
        {
            if (OnLeaveGame != null) OnLeaveGame();
        }

        void GameListener.OnOwnership(NetworkEntity entity, GamePlayer oldPlayer)
        {
            if (OnOwnership != null) OnOwnership(entity, oldPlayer);
        }

        void GameListener.OnCommand(NetworkEntity entity, GamePlayer player, string type, string category, float value)
        {
            if (OnCommand != null) OnCommand(entity, player, type, category, value);
        }

        GameObject GameListener.OnInstance(string prefabName, GamePlayer player, object data)
        {
            if (OnInstance != null) return OnInstance(prefabName, player, data);
            return null;
        }

        void GameListener.OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            if (OnDestroyInstance != null) OnDestroyInstance(gameObject, player);
        }
    }
    public class GameRoomDelegate : GameRoomListener
    {
        public delegate void OnEnterDelegate();
        public delegate void OnFailDelegate(string reason);
        public delegate void OnRoomChangeDelegate(Dictionary<string, object> props);
        public delegate void OnPlayerJoinDelegate(GamePlayer player);
        public delegate void OnPlayerLeaveDelegate(GamePlayer player);
        public delegate void OnPlayerRejoinDelegate(GamePlayer player);
        public delegate void OnPlayerChangeDelegate(GamePlayer player, Dictionary<string, object> props);
        public delegate void OnReadyPlayerDelegate(GamePlayer player);
        public delegate void OnReadyAllDelegate();

        public OnEnterDelegate OnEnter;
        public OnFailDelegate OnFail;
        public OnRoomChangeDelegate OnRoomChange;
        public OnPlayerJoinDelegate OnPlayerJoin;
        public OnPlayerLeaveDelegate OnPlayerLeave;
        public OnPlayerRejoinDelegate OnPlayerRejoin;
        public OnPlayerChangeDelegate OnPlayerChange;
        public OnReadyPlayerDelegate OnReadyPlayer;
        public OnReadyAllDelegate OnReadyAll;

        void GameRoomListener.OnEnter()
        {
            if (OnEnter != null) OnEnter();
        }

        void GameRoomListener.OnFail(string reason)
        {
            if (OnFail != null) OnFail(reason);
        }

        void GameRoomListener.OnRoomChange(Dictionary<string, object> props)
        {
            if (OnRoomChange != null) OnRoomChange(props);
        }

        void GameRoomListener.OnPlayerJoin(GamePlayer player)
        {
            if (OnPlayerJoin != null) OnPlayerJoin(player);
        }

        void GameRoomListener.OnPlayerLeave(GamePlayer player)
        {
            if (OnPlayerLeave != null) OnPlayerLeave(player);
        }

        void GameRoomListener.OnPlayerRejoin(GamePlayer player)
        {
            if (OnPlayerRejoin != null) OnPlayerRejoin(player);
        }

        void GameRoomListener.OnPlayerChange(GamePlayer player, Dictionary<string, object> props)
        {
            if (OnPlayerChange != null) OnPlayerChange(player, props);
        }

        void GameRoomListener.OnReadyPlayer(GamePlayer player)
        {
            if (OnReadyPlayer != null) OnReadyPlayer(player);
        }

        void GameRoomListener.OnReadyAll()
        {
            if (OnReadyAll != null) OnReadyAll();
        }
    }

    public class GameConnect : NamespaceListener
    {
        internal static GameListener Delegate;
        private static Namespace n;
        public static GameRoom Room;
        private static GameConnect listener;

        protected static internal Dictionary<int, Dictionary<int, NetworkEntity>> entityList = new Dictionary<int, Dictionary<int, NetworkEntity>>();

        public static GamePlayer masterClient
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
        //internal static List<int> manuallyAllocatedEntityIds = new List<int>();

        static GameConnect()
        {
            n = SceneConnect.Of(SceneRoom.Game);
            //n.compress = CompressType.Snappy;
            n.protocol = ProtocolType.Speed;
            n.messageQueue = MessageQueue.On;
            listener = new GameConnect();
            n.listener = listener;
        }
        #region flow
        // SceneConnect收到游戏房间后，执行进入游戏
        public static void JoinGame(string gameName, GameRoomListener listener)
        {
            if (stage != GameStage.None)
                throw new Exception("Game is going");
            if (listener == null)
                throw new Exception("Game Room Delegate empty");
            // 需要设定玩家委托
            // 会触发游戏进入成功，开始接受相关房间事件
            // 玩家加入，玩家修改信息
            PeerClient.isMessageQueueRunning = true;
            Room = new GameRoom(listener);
            n.Connect("game=" + gameName);
            stage = GameStage.Connect;
        }

        // 通知服务器已经能开始游戏
        public static void ReadyGame()
        {
            if (stage != GameStage.Join)
                throw new Exception("game stage need join");
            n.Emit(GameEvent.Ready, null);
            Room.localPlayer.IsReady = true;
            stage = GameStage.Ready;
            // 会发送给其他玩家准备完毕事件
            // 会触发所有玩家准备完毕事件
        }

        //开始进行场景同步，需要已经进入必要的scene中
        public static void StartInitGame(GameListener gameListener)
        {
            if (gameListener == null)
                throw new Exception("Game Delegate empty");
            if (!Room.localPlayer.IsReady)
                throw new Exception("local player need ready");
            if (stage != GameStage.ReadyAll)
                throw new Exception("game stage need join");
            Delegate = gameListener;
            listener.ResetEntityOnSerialize();
            listener.LoadScene();
            //可以开始进行游戏初始化
            stage = GameStage.Init;

            //PeerClient.isMessageQueueRunning = false;
        }

        //完成基础初始化，触发开始游戏
        public static void FinishInitGame()
        {
            if (!Room.localPlayer.IsReady)
                throw new Exception("local player need ready");
            if (stage != GameStage.Init)
                throw new Exception("game stage need start");

            // 玩家初始化数量是个人拥有的加场景对象
            Dictionary<byte, object> value = new Dictionary<byte, object>();
            value[0] = lastUsedViewSubId + lastUsedViewSubIdScene;
            EmitSync(SyncEvent.Init, Room.localPlayer.Id, value);
            Room.OnInit(Room.localPlayer.Id, lastUsedViewSubId + lastUsedViewSubIdScene);
            stage = GameStage.Start;
            Delegate.OnStart();
        }

        // 准备退出游戏
        public static void LeaveGame()
        {
            if (stage == GameStage.None)
                throw new Exception("need Join Room first");
            // 如果游戏已经完成直接退出
            if (stage == GameStage.Finish)
            {
                n.Disconnect();
                listener.OnDisconnect();
            }
            // 如果游戏还未开始，执行退出后直接退出
            else if (stage == GameStage.Join
                    || stage == GameStage.Ready
                    || stage == GameStage.Connect)
            {
                // 游戏还没开始
                n.Emit(GameEvent.Exit, null, (obj) =>
                {
                    n.Disconnect();
                    listener.OnDisconnect();
                });
            }
            // 执行退出，并执行游戏完成
            else
            {
                n.Emit(GameEvent.Exit, null, (obj) =>
                {
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
                Room.OnFail("connect is fail");
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
            PeerClient.isMessageQueueRunning = false;
        }

        public void OnReconnect(bool success)
        {
            if (!success)
            {
                Room.OnFail("reconnect is fail");
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
            //Debug.Log(code);
            switch (code)
            {
                case GameEvent.AssignPlayer:
                    stage = GameStage.Join;
                    Room.OnEnter(param[0].ConverInt());
                    break;
                case GameEvent.Ready:
                    Room.OnReady(param[0].ConverInt());
                    break;
                case GameEvent.ReadyAll:
                    stage = GameStage.ReadyAll;
                    Room.OnReadyAll();
                    break;
                case GameEvent.SwitchMaster:
                    Room.SwitchMaster(param[0].ConverInt());
                    break;
                case GameEvent.Sync:
                    OnSync((byte)param[0], param[1].ConverInt(), MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[2]));
                    break;
                case GameEvent.Ownership:
                    NetworkEntity entity = GetEntity(param[0].ConverInt(), param[1].ConverInt());
                    OnOwnership(entity, param[2].ConverInt());
                    break;
                case GameEvent.Finish:
                    stage = GameStage.Finish;
                    Delegate.OnFinish(false, (Dictionary<string, object>)param[0]);
                    break;
                case GameEvent.Join:
                    Room.OnJoin(param[0].ConverInt(), (string)param[1]);
                    //active
                    if (!(bool)param[2])
                    {
                        Room.OnLeave(param[0].ConverInt());
                    }
                    //ready
                    if ((bool)param[3])
                    {
                        Room.OnReady(param[0].ConverInt());
                    }
                    break;
                case GameEvent.Leave:
                    Room.OnLeave(param[0].ConverInt());
                    break;
                default:
                    Debug.Log("GameEvent is error:" + code);
                    break;
            }
            return null;
        }

        private static void EmitSync(byte code, int sendId, Dictionary<byte, object> value, int group = 0, Boolean reliable = true)
        {
            n.Emit(GameEvent.Sync, new object[] { code, sendId, MessagePackSerializer.Serialize(value), 0, group });
        }
        private static void EmitSync(byte code, int sendId, Dictionary<byte, object> value, int target, int group = 0, Boolean reliable = true)
        {
            n.Emit(GameEvent.Sync, new object[] { code, sendId, MessagePackSerializer.Serialize(value), target, group });
        }
        private void OnSync(byte code, int sendId, Dictionary<byte, object> value)
        {
            switch (code)
            {
                case SyncEvent.Init:
                    Room.OnInit(sendId, value[0].ConverInt());
                    break;
                case SyncEvent.Command:
                    listener.OnCommand(sendId, value);
                    break;
                case SyncEvent.Instance:
                    listener.OnInstance(sendId, value, null);
                    break;
                case SyncEvent.Destory:
                    listener.OnDestory(sendId, value[0].ConverInt(), value[1].ConverInt());
                    break;
                case SyncEvent.Serialize:
                    listener.OnSerialize(sendId, (Dictionary<byte, object>)value);
                    break;
                //case SyncEvent.ChangeRoom:
                    //    Room.OnChangeRoom(creatorId, value.ConverString());
                //    break;
                //case SyncEvent.ChangePlayer:
                    //Room.OnChangePlayer(creatorId, value.ConverString());
                //break;
                default:
                    Debug.Log("Game Sync event is error:");
                    break;
            }

        }
        public static void EmitRoom(Dictionary<string, object> props)
        {
            //EmitSync(SyncEvent.ChangeRoom, Room.localPlayer.Id, props);
        }

        public static void EmitPlayer(Dictionary<string, object> props)
        {
            //EmitSync(SyncEvent.ChangePlayer, Room.localPlayer.Id, props);
        }

        public static void TakeOver(NetworkEntity entity, Action<bool> callback)
        {
            if(entity.ownerId > 0)
            {
                callback(entity.ownerId == Room.LocalClientId);
            }
            else
            {
                n.Emit(GameEvent.OwnershipTakeover, new object[] { entity.creatorId, entity.entityId }, (object[] obj) => {
                    listener.OnOwnership(entity, (int)obj[0]);
                    callback(entity.ownerId == Room.LocalClientId);
                });
            }
        }

        public static void GiveBack(NetworkEntity entity)
        {
            if(entity.ownerId == Room.LocalClientId)
            {
                n.Emit(GameEvent.OwnershipGiveout, new object[] { entity.creatorId, entity.entityId }, (object[] obj) =>
                {
                    listener.OnOwnership(entity, (int)obj[0]);
                });
            }
        }

        public static void Members()
        {
            n.Emit(GameEvent.Members, null);
        }

        public static void RelationInstance(string prefabName, InstanceRelation relation, GameObject prefabGo, byte group, object data)
        {
            if (!inRoom)
            {
                Debug.LogError("Failed to Instantiate prefab: " + prefabName + ". Client should be in a room. Current connectionStateDetailed: " + PeerClient.connected);
                return;
            }
            // a scene object instantiated with network visibility has to contain a PhotonView
            if (prefabGo.GetComponent<NetworkEntity>() == null)
            {
                Debug.LogError("Failed to RelationInstance prefab:" + prefabName + ". Prefab must have a NetworkEntity component.");
                return;
            }

            var entitys = prefabGo.GetEntitysInChildren();
            int creatorId = 0;
            int[] entityIds = null;
            if (relation == InstanceRelation.Scene)
            {
                creatorId = Room.localPlayer.Id * -1; // 玩家创建的场景对象绑定负数的玩家id，做为创建者
                entityIds = AllocateSceneEntityIds(entitys.Length);
            }
            else if (relation == InstanceRelation.Player)
            {
                creatorId = Room.localPlayer.Id;
                entityIds = AllocateEntityIds(entitys.Length);
            }
            else if (relation == InstanceRelation.OtherPlayer)
            {
                // other
                return;
            }
            if (entityIds == null)
            {
                Debug.LogError("Failed to RelationInstance prefab: " + prefabName + ". No ViewIDs are free to use.");
                return;
            }
            Dictionary<byte, object> instantiateEvent = listener.EmitInstantiate(creatorId, prefabName, prefabGo.transform.position, prefabGo.transform.rotation, group, entityIds, data, true);
            listener.OnInstance(Room.localPlayer.Id, instantiateEvent, prefabGo);
        }

        public static void Destroy(NetworkEntity target)
        {
            if (!inRoom)
            {
                Debug.LogError("Failed to Destroy Entity");
                return;
            }
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
            if (!inRoom)
            {
                Debug.LogError("Failed to Destroy GameObject");
                return;
            }
            if (targetGo != null)
            {
                listener.RemoveInstantiatedGO(targetGo, false);
            }
            else
            {
                Debug.LogError("Destroy(targetGo) failed, cause targetEntity is null.");
            }
        }

        public static void Command(NetworkEntity entity, string type, string category, float value)
        {
            if (!inRoom)
            {
                Debug.LogError("Failed to Send Command");
                return;
            }
            Dictionary<byte, object> commandEvent = new Dictionary<byte, object>();
            commandEvent[0] = entity.entityId;
            commandEvent[1] = entity.creatorId;

            commandEvent[2] = type;
            commandEvent[3] = category;
            commandEvent[4] = value;
            EmitSync(SyncEvent.Command, Room.LocalClientId, commandEvent, entity.group, true);
        }

        //public static void RPC(NetworkEntity entity, string methodName, SyncTargets target, params object[] parameters)
        //{
        //    if (Room == null)
        //    {
        //        Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
        //        return;
        //    }
        //    else
        //    {
        //        if (target == SyncTargets.MasterClient)
        //        {
        //            listener.EmitRPC(entity, methodName, SyncTargets.Others, masterClient, parameters);
        //        }
        //        else
        //        {
        //            listener.EmitRPC(entity, methodName, target, null, parameters);
        //        }
        //    }
        //}

        /// <summary>
        /// Internal to send an RPC on given PhotonView. Do not call this directly but use: PhotonView.RPC!
        /// </summary>
        //public static void RPC(NetworkEntity entity, string methodName, GamePlayer targetPlayer, params object[] parameters)
        //{
        //    if (Room == null)
        //    {
        //        Debug.LogWarning("RPCs can only be sent in rooms. Call of \"" + methodName + "\" gets executed locally only, if at all.");
        //        return;
        //    }

        //    listener.EmitRPC(entity, methodName, SyncTargets.Others, targetPlayer, parameters);
        //}

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
                            Debug.LogError("Error: Network.SetInterestGroups was called with an illegal group number: " + g + ". The group number should be at least 1.");
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
                            Debug.LogError("Error: Network.SetInterestGroups was called with an illegal group number: " + g + ". The group number should be at least 1.");
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
        private readonly Dictionary<int, Dictionary<byte, object>> dataPerGroupReliable = new Dictionary<int, Dictionary<byte, object>>();    // only used in RunViewUpdate()
        private readonly Dictionary<int, Dictionary<byte, object>> dataPerGroupUnreliable = new Dictionary<int, Dictionary<byte, object>>();  // only used in RunViewUpdate()

        static internal short currentLevelPrefix = 0;

        /// <summary>For automatic scene syncing, the loaded scene is put into a room property. This is the name of said prop.</summary>
        protected internal const string CurrentSceneProperty = "curScn";

        static internal GameStage stage = GameStage.None;

        private readonly Dictionary<int, Dictionary<int, List<object[]>>> waitInstanceData = new Dictionary<int, Dictionary<int, List<object[]>>>();

        //public static bool UsePrefabCache = true;

        //internal IPunPrefabPool ObjectPool;

        //public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

        //private Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();

        //private readonly Dictionary<string, int> rpcShortcuts;  // lookup "table" for the index (shortcut) of an RPC name

        private void ResetEntityOnSerialize()
        {
            foreach (Dictionary<int, NetworkEntity> list in entityList.Values)
            {
                foreach(NetworkEntity entity in list.Values){
                    entity.lastOnSerializeDataSent = null;
                }
            }
        }

        internal Dictionary<byte, object> EmitInstantiate(int creatorId, string prefabName, Vector3 position, Quaternion rotation, byte group, int[] viewIDs, object data, bool isGlobalObject)
        {
            // first viewID is now also the gameobject's instantiateId
            int instantiateId = viewIDs[0];

            //TODO: reduce hashtable key usage by using a parameter array for the various values
            Dictionary<byte, object> instantiateEvent = new Dictionary<byte, object>(); // This players info is sent via ActorID
            instantiateEvent[(byte)0] = prefabName;

            //instantiateEvent[(byte)6] = PeerClient.ServerTimestamp;
            instantiateEvent[(byte)1] = instantiateId;

            instantiateEvent[(byte)2] = creatorId;

            if (position != Vector3.zero)
            {
                instantiateEvent[(byte)3] = Vector3SerializeFormatter.instance.Serialize(position);
            }

            if (rotation != Quaternion.identity)
            {
                instantiateEvent[(byte)4] = QuaternionSerializeFormatter.instance.Serialize(rotation);
            }

            if (group != 0)
            {
                instantiateEvent[(byte)5] = group;
            }

            if (currentLevelPrefix > 0)
            {
                instantiateEvent[(byte)6] = currentLevelPrefix;
            }

            // send the list of viewIDs only if there are more than one. else the instantiateId is the viewID
            if (viewIDs.Length > 1)
            {
                instantiateEvent[(byte)7] = viewIDs;
            }

            if (data != null)
            {
                instantiateEvent[(byte)8] = data;
            }

            EmitSync(SyncEvent.Instance, Room.LocalClientId, instantiateEvent);
            return instantiateEvent;
        }

        private void EmitDestroyOfInstantiate(int creatorId, int instantiateId)
        {
            Dictionary<byte, object> value = new Dictionary<byte, object>();
            value[0] = creatorId;
            value[1] = instantiateId;
            EmitSync(SyncEvent.Destory, Room.LocalClientId, value);
        }

        #region OnEvent

        private void OnOwnership(NetworkEntity entity, int newOwnerId)
        {
            GamePlayer newPlayer = Room.GetPlayerWithId(newOwnerId);
            GamePlayer oldPlayer = entity.owner;

            switch (entity.ownershipTransfer)
            {
                case OwnershipOption.Fixed:
                    Debug.LogWarning("Ownership mode == fixed. Ignoring request.");
                    break;
                case OwnershipOption.Request:
                    entity.OnTransferOwnership(newOwnerId);
                    Delegate.OnOwnership(entity, oldPlayer);
                    break;
                default:
                    break;
            }
        }

        private void OnCommand(int sendId, Dictionary<byte, object> data)
        {
            int instantiationId = (int)data[(byte)0];
            int creatorId = (int)data[(byte)1];
            NetworkEntity entity = GetEntity(creatorId, instantiationId);

            if(entity == null){
                Debug.LogWarning("Received OnCommand for view ID " + entity + ". We have no such NetworkEntity! Ignored this if you're leaving a room. State: " + PeerClient.connected);
                return;
            }

            Delegate.OnCommand(entity, Room.GetPlayerWithId(sendId), (string)data[(byte)2],  (string)data[(byte)3], (float)data[(byte)4]);
        }

        ///// <summary>
        ///// Executes a received RPC event
        ///// </summary>
        //protected internal void OnRpc(int senderID, Dictionary<byte, object> rpcData)
        //{
        //    if (rpcData == null || !rpcData.ContainsKey((byte)0))
        //    {
        //        return;
        //    }

        //    // ts: updated with "flat" event data
        //    int netEntityID = (int)rpcData[(byte)0]; // LIMITS PHOTONVIEWS&PLAYERS
        //    int otherSidePrefix = 0;    // by default, the prefix is 0 (and this is not being sent)
        //    if (rpcData.ContainsKey((byte)1))
        //    {
        //        otherSidePrefix = (short)rpcData[(byte)1];
        //    }


        //    string inMethodName = (string)rpcData[(byte)3];

        //    object[] inMethodParameters = null;
        //    if (rpcData.ContainsKey((byte)4))
        //    {
        //        inMethodParameters = (object[])rpcData[(byte)4];
        //    }

        //    if (inMethodParameters == null)
        //    {
        //        inMethodParameters = new object[0];
        //    }

        //    NetworkEntity netEntity = GetEntity(netEntityID);
        //    if (netEntity == null)
        //    {
        //        int viewOwnerId = netEntityID / MAX_ENTITY_IDS;
        //        bool owningPv = (viewOwnerId == Room.localPlayer.Id);
        //        bool ownerSent = (viewOwnerId == senderID);

        //        if (owningPv)
        //        {
        //            Debug.LogWarning("Received RPC \"" + inMethodName + "\" for netEntityID " + netEntityID + " but this NetworkEntity does not exist! View was/is ours." + (ownerSent ? " Owner called." : " Remote called.") + " By: " + senderID);
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Received RPC \"" + inMethodName + "\" for netEntityID " + netEntityID + " but this NetworkEntity does not exist! Was remote PV." + (ownerSent ? " Owner called." : " Remote called.") + " By: " + senderID + " Maybe GO was destroyed but RPC not cleaned up.");
        //        }
        //        return;
        //    }

        //    if (netEntity.prefix != otherSidePrefix)
        //    {
        //        Debug.LogError("Received RPC \"" + inMethodName + "\" on viewID " + netEntityID + " with a prefix of " + otherSidePrefix + ", our prefix is " + netEntity.prefix + ". The RPC has been ignored.");
        //        return;
        //    }

        //    // Get method name
        //    if (string.IsNullOrEmpty(inMethodName))
        //    {
        //        Debug.LogError("Malformed RPC; this should never occur. Content: " + rpcData);
        //        return;
        //    }

        //    if (PeerClient.logLevel >= LogLevel.Debug)
        //        Debug.Log("Received RPC: " + inMethodName);


        //    // SetReceiving filtering
        //    if (netEntity.group != 0 && !allowedReceivingGroups.Contains(netEntity.group))
        //    {
        //        return; // Ignore group
        //    }

        //    Type[] argTypes = new Type[0];
        //    if (inMethodParameters.Length > 0)
        //    {
        //        argTypes = new Type[inMethodParameters.Length];
        //        int i = 0;
        //        for (int index = 0; index < inMethodParameters.Length; index++)
        //        {
        //            object objX = inMethodParameters[index];
        //            if (objX == null)
        //            {
        //                argTypes[i] = null;
        //            }
        //            else
        //            {
        //                argTypes[i] = objX.GetType();
        //            }

        //            i++;
        //        }
        //    }

        //    int receivers = 0;
        //    int foundMethods = 0;
        //    if (netEntity.RpcMonoBehaviours == null || netEntity.RpcMonoBehaviours.Length == 0)
        //    {
        //        netEntity.RefreshRpcMonoBehaviourCache();
        //    }

        //    for (int componentsIndex = 0; componentsIndex < netEntity.RpcMonoBehaviours.Length; componentsIndex++)
        //    {
        //        MonoBehaviour monob = netEntity.RpcMonoBehaviours[componentsIndex];
        //        if (monob == null)
        //        {
        //            Debug.LogError("ERROR You have missing MonoBehaviours on your gameobjects!");
        //            continue;
        //        }

        //        Type type = monob.GetType();

        //        // Get [PunRPC] methods from cache
        //        List<MethodInfo> cachedRPCMethods = null;
        //        bool methodsOfTypeInCache = this.monoRPCMethodsCache.TryGetValue(type, out cachedRPCMethods);

        //        if (!methodsOfTypeInCache)
        //        {
        //            var methodsAll = type.GetMethods();
        //            var entries = new List<MethodInfo>();
        //            for (var i = 0; i < methodsAll.Length; i++)
        //            {
        //                var method = methodsAll[i];
        //                if (method is GameRPC)
        //                {
        //                    entries.Add(method);
        //                }
        //            }

        //            this.monoRPCMethodsCache[type] = entries;
        //            cachedRPCMethods = entries;
        //        }

        //        if (cachedRPCMethods == null)
        //        {
        //            continue;
        //        }

        //        // Check cache for valid methodname+arguments
        //        for (int index = 0; index < cachedRPCMethods.Count; index++)
        //        {
        //            MethodInfo mInfo = cachedRPCMethods[index];
        //            if (!mInfo.Name.Equals(inMethodName)) continue;
        //            foundMethods++;
        //            ParameterInfo[] pArray = mInfo.GetCachedParemeters();

        //            if (pArray.Length == argTypes.Length)
        //            {
        //                // Normal, PhotonNetworkMessage left out
        //                if (!ReflectClass.CheckTypeMatch(pArray, argTypes)) continue;
        //                receivers++;
        //                object result = mInfo.Invoke((object)monob, inMethodParameters);
        //                if (mInfo.ReturnType == typeof(IEnumerator))
        //                {
        //                    monob.StartCoroutine((IEnumerator)result);
        //                }
        //            }
        //            else if ((pArray.Length - 1) == argTypes.Length)
        //            {
        //                // Check for PhotonNetworkMessage being the last
        //                if (!ReflectClass.CheckTypeMatch(pArray, argTypes)) continue;
        //                if (pArray[pArray.Length - 1].ParameterType != typeof(MessageInfo)) continue;
        //                receivers++;

        //                int sendTime = (int)rpcData[(byte)2];
        //                object[] deParamsWithInfo = new object[inMethodParameters.Length + 1];
        //                inMethodParameters.CopyTo(deParamsWithInfo, 0);


        //                deParamsWithInfo[deParamsWithInfo.Length - 1] = new MessageInfo(Room.GetPlayerWithId(senderID), netEntity);

        //                object result = mInfo.Invoke((object)monob, deParamsWithInfo);
        //                if (mInfo.ReturnType == typeof(IEnumerator))
        //                {
        //                    monob.StartCoroutine((IEnumerator)result);
        //                }
        //            }
        //            else if (pArray.Length == 1 && pArray[0].ParameterType.IsArray)
        //            {
        //                receivers++;
        //                object result = mInfo.Invoke((object)monob, new object[] { inMethodParameters });
        //                if (mInfo.ReturnType == typeof(IEnumerator))
        //                {
        //                    monob.StartCoroutine((IEnumerator)result);
        //                }
        //            }
        //        }
        //    }

        //    // Error handling
        //    if (receivers != 1)
        //    {
        //        string argsString = string.Empty;
        //        for (int index = 0; index < argTypes.Length; index++)
        //        {
        //            Type ty = argTypes[index];
        //            if (argsString != string.Empty)
        //            {
        //                argsString += ", ";
        //            }

        //            if (ty == null)
        //            {
        //                argsString += "null";
        //            }
        //            else
        //            {
        //                argsString += ty.Name;
        //            }
        //        }

        //        if (receivers == 0)
        //        {
        //            if (foundMethods == 0)
        //            {
        //                Debug.LogError("NetworkEntity with ID " + netEntityID + " has no method \"" + inMethodName + "\" marked with the [PunRPC](C#) or @PunRPC(JS) property! Args: " + argsString);
        //            }
        //            else
        //            {
        //                Debug.LogError("NetworkEntity with ID " + netEntityID + " has no method \"" + inMethodName + "\" that takes " + argTypes.Length + " argument(s): " + argsString);
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogError("NetworkEntity with ID " + netEntityID + " has " + receivers + " methods \"" + inMethodName + "\" that takes " + argTypes.Length + " argument(s): " + argsString + ". Should be just one?");
        //        }
        //    }
        //}

        internal GameObject OnInstance(int sendId, Dictionary<byte, object> evData, GameObject go)
        {
            //var player = Room.GetPlayerWithId(playerId);
            // some values always present:
            string prefabName = (string)evData[(byte)0];
            //int serverTime = (int)evData[(byte)6];
            int instantiationId = (int)evData[(byte)1];
            int creatorId = (int)evData[(byte)2];
            //Debug.Log("instance:" + creatorId + "," + instantiationId);
            // 负数是该用户创建的场景物体
            var player = Room.GetPlayerWithId(sendId);

            Vector3 position;
            if (evData.ContainsKey((byte)3))
            {
                var positionBytes = (byte[])evData[(byte)3];
                position = (Vector3)Vector3SerializeFormatter.instance.Deserialize(positionBytes);
            }
            else
            {
                position = Vector3.zero;
            }

            Quaternion rotation = Quaternion.identity;
            if (evData.ContainsKey((byte)4))
            {
                var ratationBytes = (byte[])evData[(byte)4];
                rotation = (Quaternion)QuaternionSerializeFormatter.instance.Deserialize(ratationBytes);
            }

            byte group = 0;
            if (evData.ContainsKey((byte)5))
            {
                group = (byte)evData[(byte)5];
            }

            short objLevelPrefix = 0;
            if (evData.ContainsKey((byte)6))
            {
                objLevelPrefix = (short)evData[(byte)6];
            }

            int[] viewsIDs;
            if (evData.ContainsKey((byte)7))
            {
                object[] v = (object[])evData[(byte)7];
                viewsIDs = new int[v.Length];
                for (var i = 0; i < v.Length; i++)
                {
                    viewsIDs[i] = (int)v[i];
                }
            }
            else
            {
                viewsIDs = new int[1] { instantiationId };
            }

            object incomingInstantiationData;
            if (evData.ContainsKey((byte)8))
            {
                incomingInstantiationData = (object)evData[(byte)8];
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
            if (go == null)
            {
                // 统计到该用户的初始化进度中
                Room.OnInstance(sendId);
                //if (!UsePrefabCache || !PrefabCache.TryGetValue(prefabName, out resourceGameObject))
                //{
                //    resourceGameObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
                //    if (UsePrefabCache)
                //    {
                //        PrefabCache.Add(prefabName, resourceGameObject);
                //    }
                //}
                // 如果是负号玩家，则代表是场景物体，玩家为空
                go = Delegate.OnInstance(prefabName, Room.GetPlayerWithId(creatorId), incomingInstantiationData);
                if (go == null)
                {
                    Debug.LogError("error: Could not Instantiate the prefab [" + prefabName + "]. Please verify you have this gameobject in a Resources folder.");
                    return null;
                }
            }
            if(position != Vector3.zero){
                go.transform.position = position;
            }
            if(rotation != Quaternion.identity){
                go.transform.rotation = rotation;
            }

            // now modify the loaded "blueprint" object before it becomes a part of the scene (by instantiating it)
            NetworkEntity[] resourcePVs = go.GetEntitysInChildren();
            if (resourcePVs.Length != viewsIDs.Length)
            {
                throw new Exception("Error in Instantiation! The resource's Entity count is not the same as in incoming data.");
            }
            Dictionary<int, List<object[]>> waitList;
            List<object[]> waitData;
            waitInstanceData.TryGetValue(creatorId, out waitList);
            //Debug.Log("OnInstance:" + creatorId+","+instantiationId);
            for (int i = 0; i < viewsIDs.Length; i++)
            {
                // NOTE instantiating the loaded resource will keep the viewID but would not copy instantiation data, so it's set below
                // so we only set the viewID and instantiationId now. the instantiationData can be fetched
                resourcePVs[i].prefix = objLevelPrefix;
                resourcePVs[i].instantiationId = instantiationId;
                resourcePVs[i].isRuntimeInstantiated = true;
                resourcePVs[i].creatorId = creatorId;
                // 注册entity
                resourcePVs[i].entityId = viewsIDs[i];

                // 延后执行
                if(waitList != null && waitList.TryGetValue(viewsIDs[i], out waitData))
                {
                    waitData.ForEach((object[] obj) => resourcePVs[i].OnSerializeRead(readStream, player, obj, objLevelPrefix));
                    waitList.Remove(viewsIDs[i]);
                }
            }

            //this.StoreInstantiationData(instantiationId, incomingInstantiationData);

            //// load the resource and set it's values before instantiating it:
            //GameObject go = (GameObject)GameObject.Instantiate(resourceGameObject, position, rotation);

            //for (int i = 0; i < viewsIDs.Length; i++)
            //{
            //    // NOTE instantiating the loaded resource will keep the viewID but would not copy instantiation data, so it's set below
            //    // so we only set the viewID and instantiationId now. the instantiationData can be fetched
            //    resourcePVs[i].entityId = 0;
            //    resourcePVs[i].prefix = -1;
            //    resourcePVs[i].prefixBackup = -1;
            //    resourcePVs[i].instantiationId = -1;
            //    resourcePVs[i].isRuntimeInstantiated = false;
            //}

            //this.RemoveInstantiationData(instantiationId);

            return go;
        }

        private void OnDestory(int sendId, int creatorId, int entityId)
        {

            NetworkEntity entity = GetEntity(creatorId, entityId);
            if(entity != null)
            {
                RemoveInstantiatedGO(entity.gameObject, true);
            }
            else
            {
                if (PeerClient.logLevel >= LogLevel.Error) Debug.LogError("Ev Destroy Failed. Could not find Entity with instantiationId " + entityId);
            }

        }
        #endregion

        //private static Dictionary<int, object[]> tempInstantiationData = new Dictionary<int, object[]>();

        //private void StoreInstantiationData(int instantiationId, object[] instantiationData)
        //{
        //    tempInstantiationData[instantiationId] = instantiationData;
        //}

        //public static object[] FetchInstantiationData(int instantiationId)
        //{
        //    object[] data = null;
        //    if (instantiationId == 0)
        //    {
        //        return null;
        //    }

        //    tempInstantiationData.TryGetValue(instantiationId, out data);
        //    return data;
        //}

        //private void RemoveInstantiationData(int instantiationId)
        //{
        //    tempInstantiationData.Remove(instantiationId);
        //}

        public void DestroyPlayerObjects(int playerId)
        {
            if (playerId <= 0)
            {
                Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
                return;
            }

            // locally cleaning up that player's objects
            HashSet<GameObject> playersGameObjects = new HashSet<GameObject>();
            foreach(Dictionary<int, NetworkEntity> list in entityList.Values)
            {
                foreach (NetworkEntity entity in list.Values)
                {
                    if (entity != null && entity.creatorId == playerId)
                    {
                        playersGameObjects.Add(entity.gameObject);
                    }
                }
            }

            // any non-local work is already done, so with the list of that player's objects, we can clean up (locally only)
            foreach (GameObject gameObject in playersGameObjects)
            {
                RemoveInstantiatedGO(gameObject, true);
            }

            // with ownership transfer, some objects might lose their owner.
            // in that case, the creator becomes the owner again. every client can apply this. done below.
            foreach (Dictionary<int, NetworkEntity> list in entityList.Values)
            {
                foreach (NetworkEntity entity in list.Values)
                {
                    if (entity.ownerId == playerId)
                    {
                        entity.ownerId = entity.creatorId;
                    }
                }
            }
        }

        protected void DestroyAll()
        {
            //if (tempInstantiationData.Count > 0)
            //{
            //    Debug.LogWarning("It seems some instantiation is not completed, as instantiation data is used. You should make sure instantiations are paused when calling this method. Cleaning now, despite this.");
            //}

            HashSet<GameObject> instantiatedGos = new HashSet<GameObject>();
            foreach (Dictionary<int, NetworkEntity> list in entityList.Values)
            {
                foreach (NetworkEntity entity in list.Values)
                {
                    if (entity.isRuntimeInstantiated)
                    {
                        instantiatedGos.Add(entity.gameObject); // HashSet keeps each object only once
                    }
                }
            }

            foreach (GameObject go in instantiatedGos)
            {
                RemoveInstantiatedGO(go, true);
            }

            //tempInstantiationData.Clear(); // should be empty but to be safe we clear (no new list needed)
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
                return;
            }

            NetworkEntity viewZero = entitys[0];
            int creatorId = viewZero.creatorId;            // creatorId of obj is needed to delete EvInstantiate (only if it's from that user)
            int instantiationId = viewZero.instantiationId;     // actual, live InstantiationIds start with 1 and go up

            // Don't remove the Instantiation from the server, if it doesn't have a proper ID
            if (instantiationId < 1)
            {
                Debug.LogError("Failed to 'network-remove' GameObject because it is missing a valid InstantiationId on view: " + viewZero + ". Not Destroying GameObject or PhotonViews!");
                return;
            }

            // Don't remove GOs that are owned by others (unless this is the master and the remote player left)
            if (!localOnly)
            {
                if (viewZero.isMine)
                {
                    EmitDestroyOfInstantiate(creatorId, instantiationId);
                }
            }


            // cleanup PhotonViews and their RPCs events (if not localOnly)
            for (int j = entitys.Length - 1; j >= 0; j--)
            {
                NetworkEntity entity = entitys[j];
                if (entity == null)
                {
                    continue;
                }

                if (entity.instantiationId >= 1)
                {
                    LocalCleanEntity(entity);
                }
            }

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Network destroy Instantiated GO: " + go.name);
            }

            // todo 通知用户
            Delegate.OnDestroyInstance(go, Room.GetPlayerWithId(creatorId));
        }

        public static bool LocalCleanEntity(NetworkEntity entity)
        {
            entity.removedFromLocalList = true;
            Dictionary<int, NetworkEntity> list;
            if (entityList.TryGetValue(entity.creatorId, out list))
            {
                return list.Remove(entity.entityId);
            }
            else
            {
                return false;
            }
        }

        //
        public static NetworkEntity GetEntity(int creatorId, int entityId)
        {
            NetworkEntity result = null;
            Dictionary<int, NetworkEntity> list;
            if(entityList.TryGetValue(creatorId, out list)){
                list.TryGetValue(entityId, out result);
            }

            //if (result == null)
            //{
            //    NetworkEntity[] entitys = GameObject.FindObjectsOfType(typeof(NetworkEntity)) as NetworkEntity[];

            //    for (int i = 0; i < entitys.Length; i++)
            //    {
            //        NetworkEntity entity = entitys[i];
            //        if (entity.entityId == entityId)
            //        {
            //            if (entity.didAwake)
            //            {
            //                Debug.LogWarning("Had to lookup view that wasn't in entityList: " + entity);
            //            }
            //            return entity;
            //        }
            //    }
            //}

            return result;
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

            NetworkEntity entity = GetEntity(netEntity.creatorId, netEntity.entityId);
            if (entity != null)
            {
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

            //Debug.Log("register:"+netEntity.creatorId+","+netEntity.entityId);
            // Debug.Log("adding view to known list: " + netView);
            AddEntity(netEntity.creatorId, netEntity);
            //Debug.LogError("view being added. " + netView);   // Exit Games internal log

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Registered NetworkEntity: " + netEntity.entityId);
            }
        }

        private static void AddEntity(int creatorId, NetworkEntity entity)
        {
            Dictionary<int, NetworkEntity> list;
            if (!entityList.TryGetValue(creatorId, out list))
            {
                list = new Dictionary<int, NetworkEntity>();
                entityList.Add(creatorId, list);
            }
            list.Add(entity.entityId, entity);
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

        //internal void EmitRPC(NetworkEntity entity, string methodName, SyncTargets target, GamePlayer player, params object[] parameters)
        //{
        //    if (blockSendingGroups.Contains(entity.group))
        //    {
        //        return; // Block sending on this group
        //    }

        //    if (entity.entityId < 1)
        //    {
        //        Debug.LogError("Illegal entity ID:" + entity.entityId + " method: " + methodName + " GO:" + entity.gameObject.name);
        //    }

        //    if (PeerClient.logLevel >= LogLevel.Debug)
        //    {
        //        Debug.Log("Sending RPC \"" + methodName + "\" to target: " + target + " or player:" + player + ".");
        //    }


        //    //ts: changed RPCs to a one-level hashtable as described in internal.txt
        //    Dictionary<byte, object> rpcEvent = new Dictionary<byte, object>();
        //    rpcEvent[(byte)0] = (int)entity.entityId; // LIMITS NETWORKVIEWS&PLAYERS
        //    if (entity.prefix > 0)
        //    {
        //        rpcEvent[(byte)1] = (short)entity.prefix;
        //    }


        //    // send name or shortcut (if available)
        //    int shortcut = 0;
        //    if (rpcShortcuts.TryGetValue(methodName, out shortcut))
        //    {
        //        rpcEvent[(byte)4] = (byte)shortcut; // LIMITS RPC COUNT
        //    }
        //    else
        //    {
        //        rpcEvent[(byte)2] = methodName;
        //    }

        //    if (parameters != null && parameters.Length > 0)
        //    {
        //        rpcEvent[(byte)3] = (object[])parameters;
        //    }


        //    // if sent to target player, this overrides the target
        //    if (player != null)
        //    {
        //        if (Room.localPlayer.Id == player.Id)
        //        {
        //            this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //        }
        //        else
        //        {
        //            //RaiseEventOptions options = new RaiseEventOptions() { TargetActors = new int[] { player.Id } };
        //            EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, player.Id, entity.group, false);
        //        }

        //        return;
        //    }

        //    // send to a specific set of players
        //    if (target == SyncTargets.All)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, false);

        //        // Execute local
        //        this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //    }
        //    else if (target == SyncTargets.Others)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, false);
        //    }
        //    else if (target == SyncTargets.AllBuffered)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, true);

        //        // Execute local
        //        this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //    }
        //    else if (target == SyncTargets.OthersBuffered)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, true);
        //    }
        //    else if (target == SyncTargets.MasterClient)
        //    {
        //        if (Room.isMasterClient)
        //        {
        //            this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //        }
        //        else
        //        {
        //            //RaiseEventOptions options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
        //            EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, Room.masterClient.Id, entity.group, false);
        //        }
        //    }
        //    else if (target == SyncTargets.AllViaServer)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group, Receivers = ReceiverGroup.All };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, false);
        //        if (PeerClient.offlineMode)
        //        {
        //            this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //        }
        //    }
        //    else if (target == SyncTargets.AllBufferedViaServer)
        //    {
        //        //RaiseEventOptions options = new RaiseEventOptions() { InterestGroup = (byte)entity.group, Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCache };
        //        EmitSync(SyncEvent.RPC, Room.localPlayer.Id, rpcEvent, 0, entity.group, true);
        //        if (PeerClient.offlineMode)
        //        {
        //            this.OnRpc(Room.localPlayer.Id, rpcEvent);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("Unsupported target enum: " + target);
        //    }
        //}

        public static int ObjectsInOneUpdate = 20;

        #region sync entity
        internal static void Update()
        {
            listener.UpdateEntity();
        }

        private void UpdateEntity()
        {
            if (PeerClient.offlineMode || stage != GameStage.Start)
            {
                return;
            }

            // no need to send OnSerialize messages while being alone (these are not buffered anyway)
            //if (Room.mActors.Count <= 1)
            //{
            //    return;
            //}


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

            var enumerator = entityList.GetEnumerator();   // replacing foreach (PhotonView view in this.photonViewList.Values) for memory allocation improvement
            while (enumerator.MoveNext())
            {
                var list = enumerator.Current.Value;
                var listEnumerator = list.GetEnumerator();
                List<int> toRemove = null;
                while (listEnumerator.MoveNext())
                {
                    NetworkEntity entity = listEnumerator.Current.Value;

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

                    // 根据isMine确定同步关系
                    if (entity.synchronization == EntitySynchronization.Off || entity.isMine == false || entity.gameObject.activeInHierarchy == false)
                    {
                        continue;
                    }

                    if (blockSendingGroups.Contains(entity.group))
                    {
                        continue; // Block sending on this group
                    }

                    object[] evData = entity.OnSerializeWrite(pStream, Room.localPlayer);
                    if (evData == null)
                    {
                        continue;
                    }
                    evData = SerializeStream(ref evData);

                    if (entity.synchronization == EntitySynchronization.Reliable || entity.mixedModeIsReliable)
                    {
                        Dictionary<byte, object> groupHashtable = null;
                        bool found = this.dataPerGroupReliable.TryGetValue(entity.group, out groupHashtable);
                        if (!found)
                        {
                            groupHashtable = new Dictionary<byte, object>(ObjectsInOneUpdate);
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
                            EmitSync(SyncEvent.Serialize, Room.LocalClientId, groupHashtable, entity.group, true);
                            //Debug.Log("SendSerializeReliable (10) " + PhotonNetwork.networkingPeer.ByteCountLastOperation);
                            groupHashtable.Clear();
                        }
                    }
                    else
                    {
                        Dictionary<byte, object> groupHashtable = null;
                        bool found = this.dataPerGroupUnreliable.TryGetValue(entity.group, out groupHashtable);
                        if (!found)
                        {
                            groupHashtable = new Dictionary<byte, object>(ObjectsInOneUpdate);
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

                            EmitSync(SyncEvent.Serialize, Room.LocalClientId, groupHashtable, entity.group, false);
                            groupHashtable.Clear();
                        }
                    }
                }
                if (toRemove != null)
                {
                    for (int idx = 0, count = toRemove.Count; idx < count; ++idx)
                    {
                        list.Remove(toRemove[idx]);
                    }
                }
            }   // all views serialized


            // if we didn't produce anything to send, don't do it
            if (countOfUpdatesToSend == 0)
            {
                return;
            }


            foreach (int groupId in this.dataPerGroupReliable.Keys)
            {
                //options.InterestGroup = (byte)groupId;
                Dictionary<byte, object> groupHashtable = this.dataPerGroupReliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }

                //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                if (currentLevelPrefix >= 0)
                {
                    groupHashtable[(byte)0] = currentLevelPrefix;
                }

                EmitSync(SyncEvent.Serialize, Room.LocalClientId, groupHashtable, groupId, true);
                groupHashtable.Clear();
            }
            foreach (int groupId in this.dataPerGroupUnreliable.Keys)
            {
                //options.InterestGroup = (byte)groupId;
                Dictionary<byte, object> groupHashtable = this.dataPerGroupUnreliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }

                //groupHashtable[(byte)0] = PeerClient.ServerTimestamp;
                if (currentLevelPrefix >= 0)
                {
                    groupHashtable[(byte)0] = currentLevelPrefix;
                }

                EmitSync(SyncEvent.Serialize, Room.LocalClientId, groupHashtable, groupId, false);
                groupHashtable.Clear();
            }
        }

        private void OnSerialize(int sendId, Dictionary<byte, object> serializeData)
        {
            short remoteLevelPrefix = -1;
            byte initialDataIndex = 10;
            if (serializeData.ContainsKey((byte)0))
            {
                remoteLevelPrefix = (short)serializeData[(byte)0];
            }
            var s = initialDataIndex;

            object data;
            do
            {
                var result = serializeData.TryGetValue(s, out data);
                if (!result) break;
                var d = data as object[];

                int entityId = (int)d[NetworkEntity.SyncViewId];
                int creatorId = (int)d[NetworkEntity.SyncCreatorId];
                //Debug.Log("serialize:" + creatorId + ',' + entityId);
                NetworkEntity entity = GetEntity(creatorId, entityId);
                if (entity == null)
                {
                    if(Room.GetPlayerWithId(Math.Abs(creatorId)) == null)
                    {
                        Debug.LogWarning("Received OnSerialization for view ID " + entityId + ". We have no such NetworkEntity! Ignored this if you're leaving a room. State: " + PeerClient.connected);
                    }
                    else
                    {
                        Dictionary<int, List<object[]>> map;
                        List<object[]> list;
                        if(!waitInstanceData.TryGetValue(creatorId, out map))
                        {
                            map = new Dictionary<int, List<object[]>>();
                            waitInstanceData.Add(creatorId, map);
                        }
                        if (!map.TryGetValue(entityId, out list))
                        {
                            list = new List<object[]>();
                            map.Add(entityId, list);
                        }
                        list.Add(d);
                        Debug.Log("Wait Received Instance" + entityId);
                    }
                    return;
                }

                // SetReceiving filtering
                if (entity.group != 0 && !allowedReceivingGroups.Contains(entity.group))
                {
                    return; // Ignore group
                }
                entity.OnSerializeRead(readStream, Room.GetPlayerWithId(sendId), d, remoteLevelPrefix);
                s++;
            } while (true);
        }

        #endregion

        #region scene
        internal protected void LoadScene()
        {
            Dictionary<string, object> setScene = new Dictionary<string, object>();
            setScene[CurrentSceneProperty] = (string)SceneManagerHelper.ActiveSceneName;

            Room.InternalProperties(setScene);
            currentLevelPrefix = short.Parse(SceneManagerHelper.ActiveSceneBuildIndex.ToString());
        }
        #endregion

        private void ClearRoom()
        {
            Room.Clear();
            Room = null;
            allowedReceivingGroups = new HashSet<byte>();
            blockSendingGroups = new HashSet<byte>();
            DestroyAll();
            Delegate.OnLeaveGame();
        }

        #region Allocate
        internal static int AllocateEntityId()
        {
            int manualId = AllocateEntityId(Room.localPlayer.Id);
            //manuallyAllocatedEntityIds.Add(manualId);
            return manualId;
        }

        internal static int[] AllocateEntityIds(int countOfNewEntitys)
        {
            int[] entityIds = new int[countOfNewEntitys];
            for (int entity = 0; entity < countOfNewEntitys; entity++)
            {
                entityIds[entity] = AllocateEntityId(Room.localPlayer.Id);
            }

            return entityIds;
        }

        internal static int AllocateSceneEntityId()
        {
            //if (!isMasterClient)
            //{
            //    Debug.LogError("Only the Master Client can AllocateSceneEntityId(). Check isMasterClient!");
            //    return -1;
            //}

            int manualId = AllocateEntityId(Room.localPlayer.Id * -1);
            //manuallyAllocatedEntityIds.Add(manualId);
            return manualId;
        }

        internal static int[] AllocateSceneEntityIds(int countOfNewEntitys)
        {
            int[] entityIds = new int[countOfNewEntitys];
            for (int entity = 0; entity < countOfNewEntitys; entity++)
            {
                entityIds[entity] = AllocateEntityId(Room.localPlayer.Id * -1);
            }

            return entityIds;
        }

        internal static int AllocateEntityId(int ownerId)
        {
            if(ownerId > 0){
                lastUsedViewSubId += 1;
                return lastUsedViewSubId;
            }
            else
            {
                lastUsedViewSubIdScene += 1;
                return lastUsedViewSubIdScene;
            }
            //int newSubId = ownerId > 0 ? lastUsedViewSubId : lastUsedViewSubIdScene;
            //int newEntityId;
            //for (int i = newSubId; i < MAX_ENTITY_IDS; i++)
            //{
            //    newSubId = (i + 1) % MAX_ENTITY_IDS;
            //    if (newSubId == 0)
            //    {
            //        continue;   // avoid using subID 0
            //    }

            //    newEntityId = newSubId + ownerIdOffset;
            //    if (entityList.ContainsKey(newEntityId))
            //    {
            //        continue;
            //    }
            //    //if (manuallyAllocatedEntityIds.Contains(newEntityId))
            //    //{
            //    //    continue;
            //    //}
            //    if (ownerId > 0)
            //    {
            //        lastUsedViewSubId = newSubId;
            //    }
            //    else
            //    {
            //        lastUsedViewSubIdScene = newSubId;
            //    }
            //    //Debug.Log(newEntityId);
            //    //Debug.Log(newSubId);
            //    return newEntityId;
            //}

            //throw new Exception(string.Format("AllocateEntityId() failed. User {0} is out of subIds, as all Entity are used.", ownerId));
        }

        //internal static void UnAllocateEntityId(int entityId)
        //{
        //    //manuallyAllocatedEntityIds.Remove(entityId);

        //    if (entityList.ContainsKey(entityId))
        //    {
        //        Debug.LogWarning(string.Format("UnAllocateEntityID() should be called after the Entity was destroyed (GameObject.Destroy()). entityId: {0} still found in: {1}", entityId, entityList[entityId]));
        //    }
        //}
        #endregion

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

    }
}
