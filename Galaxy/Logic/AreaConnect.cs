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
    internal class AreaEvent
    {
        public const byte AcceptGroup = 0;
        public const byte AcceptLevel = 1;
        public const byte Sync = 2;
        public const byte Instance = 3;
        public const byte ChangeInfo = 4;
        public const byte ChangeLocation = 5;
        public const byte Destroy = 6;
        public const byte Ownership = 7;
        public const byte Survey = 8;
        public const byte Serialize = 9;
        public const byte Request = 10;
        public const byte Response = 11;
    }

    internal class SyncEvent
    {
        public const byte Command = 1;
        public const byte Affect = 2;
    }

    public enum InstanceRelation:byte
    {
        Player,
        Scene,
        OtherPlayer
    }

    public interface AreaListener
    {
        GameObject OnInstance(string prefabName, InstanceRelation relation, GamePlayer player, Vector3 position, Quaternion rotation);
        void OnDestroyInstance(GameObject gameObject, GamePlayer player);
        void OnRequest(GamePlayer player, byte code, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback);
    }
    public interface PlayerFactory
    {
        GamePlayer GetPlayer(string userId);
        void OnInstance(string userId);
    }
    public class AreaDelegate : AreaListener
    {
        public delegate GameObject OnInstanceDelegate(string prefabName, InstanceRelation relation, GamePlayer player, Vector3 position, Quaternion rotation);
        public delegate void OnDestroyInstanceDelegate(GameObject gameObject, GamePlayer player);
        public delegate void OnRequestDelegate(GamePlayer player, byte code, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback);

        public OnInstanceDelegate OnInstanceEvent;
        public OnDestroyInstanceDelegate OnDestroyInstanceEvent;
        public OnRequestDelegate OnRequestEvent;

        internal void Bind(AreaListener listener)
        {
            OnInstanceEvent = listener.OnInstance;
            OnDestroyInstanceEvent = listener.OnDestroyInstance;
            OnRequestEvent = listener.OnRequest;
        }

        public GameObject OnInstance(string prefabName, InstanceRelation relation, GamePlayer player, Vector3 position, Quaternion rotation)
        {
            if (OnInstanceEvent != null) return OnInstanceEvent(prefabName, relation, player, position, rotation);
            return null;
        }

        public void OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            if (OnDestroyInstanceEvent != null) OnDestroyInstanceEvent(gameObject, player);
        }

        public void OnRequest(GamePlayer player, byte code, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback)
        {
            if (OnRequestEvent != null) OnRequestEvent(player, code, value, callback);
        }
    }

    public class AreaConnect : NetworkListener, NamespaceListener
    {
        internal static AreaDelegate Delegate;
        private static Namespace n;
        private static AreaConnect listener;
        public static AreaConnect Listener
        {
            get
            {
                return listener;
            }
        }
        private Action<bool> OnConnectAction;

        private PlayerFactory players;

        protected static internal Dictionary<string, NetworkEsse> esseList = new Dictionary<string, NetworkEsse>();

        internal static HashidsNet.Hashids userHash = null;
        internal static HashidsNet.Hashids sceneHash = null;
        internal static string localId = "";
        internal static int lastUsedViewSubId = 0;
        internal static int lastUsedViewSubIdScene = 0;
        internal static int lastRequestId = 0;

        private static HashSet<string> allowedReceivingGroups = new HashSet<string>();
        private static HashSet<string> blockSendingGroups = new HashSet<string>();

        private readonly StreamBuffer readStream = new StreamBuffer(false, null);    // only used in OnSerializeRead()
        private readonly StreamBuffer pStream = new StreamBuffer(true, null);        // only used in OnSerializeWrite()
        private readonly Dictionary<string, Dictionary<byte, List<object[]>>> dataPerGroupReliable = new Dictionary<string, Dictionary<byte, List<object[]>>>();    // only used in RunViewUpdate()
        private readonly Dictionary<string, Dictionary<byte, List<object[]>>> dataPerGroupUnreliable = new Dictionary<string, Dictionary<byte, List<object[]>>>();  // only used in RunViewUpdate()

        private readonly Dictionary<string, Action<Dictionary<byte, object>>> requestCache = new Dictionary<string, Action<Dictionary<byte, object>>>();
        private readonly Dictionary<string, List<object[]>> waitInstanceData = new Dictionary<string, List<object[]>>();


        static AreaConnect()
        {
            n = PeerClient.Of(NamespaceId.Area);
            n.compress = true;
            n.protocol = ProtocolType.Speed;
            //n.messageQueue = MessageQueue.On;
            listener = new AreaConnect();
            n.listener = listener;
            Delegate = new AreaDelegate();
            listener.OnConnectEvent += (success) =>
            {
                if (listener.OnConnectAction != null) listener.OnConnectAction(success);
            };
        }

        public static void Join(string token, string userId, PlayerFactory factory, Action<bool> action)
        {
            localId = userId;
            listener.players = factory;
            userHash = new HashidsNet.Hashids(token + localId);
            sceneHash = new HashidsNet.Hashids(token);

            if (PeerClient.offlineMode){
                action(true);
            } else {
                listener.OnConnectAction = action;
                n.Connect("token=" + token);
            }
        }

        public static void Leave()
        {
            n.Disconnect();
        }

        public static void StartInit(AreaListener listener)
        {
            Delegate.Bind(listener);
            Listener.ResetEntityOnSerialize();
        }

        public static int FinishInit()
        {
            // 玩家初始化数量是个人拥有的加场景对象
            return lastUsedViewSubId + lastUsedViewSubIdScene;
        }

        public void OnError(string message)
        {
            Debug.Log(message);
        }

        public object[] OnEvent(byte code, object[] param)
        {
            //Debug.Log(code);
            switch (code)
            {
                case AreaEvent.Instance:
                    // userId, hash, group, level, value
                    listener.OnInstance((string)param[0], (string)param[1], (string)param[2], (byte)param[3], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[4]), null);
                    break;
                case AreaEvent.ChangeInfo:
                    
                    break;
                case AreaEvent.ChangeLocation:
                    break;
                case AreaEvent.Serialize:
                    listener.OnSerialize(players.GetPlayer((string)param[0]), MessagePackSerializer.Deserialize<List<object[]>>((byte[])param[1]), (string)param[2], (byte)param[3]);
                    break;
                case AreaEvent.Sync:
                    // code, userId, value
                    listener.OnSync((byte)param[0], (string)param[1], (byte[])param[2]);
                    break;
                case AreaEvent.Ownership:
                    // esse, userId
                    listener.OnOwnership(Get((string)param[0]), (string)param[1]);
                    break;
                case AreaEvent.Destroy:
                    // esse
                    listener.OnDestroy(Get((string)param[0]));
                    break;
                case AreaEvent.Survey:
                    // hash, data
                    listener.OnSurvey(Get((string)param[0]), (string)param[1], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[2]));
                    break;
                case AreaEvent.Request:
                    // userId, key, data
                    listener.OnRequest(players.GetPlayer((string)param[0]), (byte)param[1], (string)param[2], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[3]));
                    break;
                case AreaEvent.Response:
                    // userId, key, data
                    listener.OnResponse(players.GetPlayer((string)param[0]), (byte)param[1], (string)param[2], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[3]));
                    break;
                default:
                    Debug.Log("AreaEvent is error:" + code);
                    break;
            }
            return null;
        }

        private static void Emit(byte code, NetworkEsse esse, object value, Action<object[]> callback)
        {
            object[] p;
            if (value != null) {
                p = new object[] { esse.hash, esse.group, esse.level, value };
            } else {
                p = new object[] { esse.hash, esse.group, esse.level };
            }
            n.Emit(code, p, callback);
        }

        private static void EmitSync(byte code, Dictionary<byte, object> value, string group, byte level)
        {
            n.Emit(AreaEvent.Sync, new object[] { code, MessagePackSerializer.Serialize(value), group, level });
        }
        private static void EmitSyncCallback(byte code, Dictionary<byte, object> value, string group, byte level, Action<object[]> callback = null)
        {
            n.Emit(AreaEvent.Sync, new object[] { code, MessagePackSerializer.Serialize(value), group, level }, callback);
        }
        private static void EmitSerialize(List<object[]> value, string group, byte level)
        {
            byte[] info = MessagePackSerializer.Serialize(value);
            n.Emit(AreaEvent.Serialize, new object[] { info, group, level });
        }
        private void OnSync(byte code, string sendId, byte[] value)
        {
            GamePlayer player = players.GetPlayer(sendId);
            switch (code)
            {
                case SyncEvent.Command:
                    listener.OnCommand(player, MessagePackSerializer.Deserialize<Dictionary<byte, object>>(value));
                    break;
                case SyncEvent.Affect:
                    listener.OnAffect(player, MessagePackSerializer.Deserialize<Dictionary<byte, object>>(value));
                    break;
                default:
                    Debug.Log("Sync event is error:");
                    break;
            }
        }

        public static void Ownership(NetworkEsse esse, bool release, Action<bool> callback)
        {
            if (PeerClient.offlineMode){
                callback(!release);
            } else {
                Emit(AreaEvent.Ownership, esse, release, (object[] obj) => {
                    listener.OnOwnership(esse, (string)obj[0]);
                    callback(esse.ownerId == localId);
                });
            }
        }

        public static void Survey(NetworkEsse esse, Dictionary<byte, object> value, int seconds)
        {
            var hash = sceneHash.EncodeLong(PeerClient.LocalTimestamp);

            if (PeerClient.offlineMode){
                listener.OnEvent(AreaEvent.Survey, new object[]{ esse.hash, hash, MessagePackSerializer.Serialize(value) });
            } else{
                n.Emit(AreaEvent.Survey, new object[]{ esse.hash, esse.group, esse.level, hash, MessagePackSerializer.Serialize(value), seconds });
            }
        }

        public static void RelationInstance(NetworkEsse esse, string prefabName, InstanceRelation relation, GameObject prefabGo)
        {
            if (relation == InstanceRelation.Scene)
            {
                esse.creatorId = localId;
                esse.hash = AllocateSceneHash();
            }
            else if (relation == InstanceRelation.Player)
            {
                esse.creatorId = localId;
                esse.hash = AllocateHash();
            }
            else if (relation == InstanceRelation.OtherPlayer)
            {
                // other
                return;
            }
            Debug.Log(esse);
            Dictionary<byte, object> instantiateEvent = listener.EmitInstantiate(esse, prefabName, prefabGo, relation);
            listener.OnInstance(esse.CreatorId, esse.hash, esse.group, esse.level, instantiateEvent, prefabGo);
        }

        public static void Destroy(NetworkEsse esse)
        {
            if (PeerClient.offlineMode) {
                listener.RemoveInstantiated(esse, true);
            } else if (n.State != ConnectionState.Connected){
                Debug.LogError("Failed to Destroy Entity");
                return;
            } else {
                listener.RemoveInstantiated(esse, false);
            }
        }

        public static void Command(NetworkEsse esse, Action callback, object type, object category, object value)
        {
            if (PeerClient.offlineMode) {
                callback();
                return;
            } else if (n.State != ConnectionState.Connected){
                Debug.LogError("Failed to Send Command");
                return;
            }
            Dictionary<byte, object> commandEvent = new Dictionary<byte, object>();
            commandEvent[0] = esse.hash;

            commandEvent[1] = type;
            commandEvent[2] = category;
            commandEvent[3] = value;
            EmitSyncCallback(SyncEvent.Command, commandEvent, esse.group, esse.level, (object[] obj) => {
                callback();
            });
        }

        public static void Request(GamePlayer player, byte code, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback)
        {
            var key = userHash.Encode(lastRequestId++);
            listener.requestCache[key] = callback;
            n.Emit(AreaEvent.Request, new object[] { player.UserId, code, key, MessagePackSerializer.Serialize(value) });
        }

        public static void SetGroups(string[] disableGroups, string[] enableGroups)
        {
            //listener.EmitChangeGroups(disableGroups, enableGroups);
            if (PeerClient.offlineMode) {
                
            } else {
                n.Emit(AreaEvent.AcceptGroup, new object[] { disableGroups, enableGroups });
            }
        }
        public static void SetLevel(byte level)
        {
            if (PeerClient.offlineMode) {
                
            } else {
                n.Emit(AreaEvent.AcceptLevel, new object[] { level });
            }
        }
        public static void ChangeInfo(NetworkEsse esse)
        {
            GameObject go = esse.gameObject;
            Dictionary<byte, object> instantiateEvent = new Dictionary<byte, object>();

            instantiateEvent[(byte)0] = esse.prefabs;

            if (go.transform.position != Vector3.zero)
            {
                instantiateEvent[(byte)1] = Vector3SerializeFormatter.instance.Serialize(go.transform.position);
            }

            if (go.transform.rotation != Quaternion.identity)
            {
                instantiateEvent[(byte)2] = QuaternionSerializeFormatter.instance.Serialize(go.transform.rotation);
            }

            var data = esse.GetData(listener.pStream);

            if (data)
            {
                instantiateEvent[(byte)3] = listener.pStream.ToArray();
            }

            n.Emit(AreaEvent.ChangeInfo, new object[] { esse.hash, esse.group, esse.level, MessagePackSerializer.Serialize(instantiateEvent) });
        }
        public static void ChangeLocation(NetworkEsse esse, string group, byte level)
        {
            if (PeerClient.offlineMode) {
                
            } else {
                n.Emit(AreaEvent.ChangeLocation, new object[] { esse.hash, esse.group, esse.level, group, level });
            }
            esse.group = group;
            esse.level = level;
        }
        private void ResetEntityOnSerialize()
        {
            foreach (NetworkEsse esse in esseList.Values)
            {
                esse.lastOnSerializeDataSent = null;
            }
        }

        internal Dictionary<byte, object> EmitInstantiate(NetworkEsse esse, string prefabName, GameObject go, InstanceRelation relation)
        {
            Dictionary<byte, object> instantiateEvent = new Dictionary<byte, object>();

            instantiateEvent[(byte)0] = prefabName;
            instantiateEvent[(byte)1] = (byte)relation;
            if (go.transform.position != Vector3.zero)
            {
                instantiateEvent[(byte)2] = Vector3SerializeFormatter.instance.Serialize(go.transform.position);
            }

            if (go.transform.rotation != Quaternion.identity)
            {
                instantiateEvent[(byte)3] = QuaternionSerializeFormatter.instance.Serialize(go.transform.rotation);
            }

            var data = esse.GetData(pStream);

            if (data)
            {
                instantiateEvent[(byte)4] = pStream.ToArray();
            }
            if (!PeerClient.offlineMode)
                n.Emit(AreaEvent.Instance, new object[] { esse.hash, esse.group, esse.level, MessagePackSerializer.Serialize(instantiateEvent) });
            return instantiateEvent;
        }

        private void EmitDestroy(NetworkEsse esse)
        {
            Emit(AreaEvent.Destroy, esse, null, null);
        }

        #region OnEvent

        private void OnOwnership(NetworkEsse esse, string newOwnerId)
        {
            // 已经被销毁
            if (esse == null) return;

            switch (esse.ownershipTransfer)
            {
                case OwnershipOption.Fixed:
                    Debug.LogWarning("Ownership mode == fixed. Ignoring request.");
                    break;
                case OwnershipOption.Request:
                    esse.OnTransferOwnership(players.GetPlayer(newOwnerId));
                    break;
                default:
                    break;
            }
        }

        private void OnSurvey(NetworkEsse esse, string hash, Dictionary<byte, object> data)
        {
            esse.OnSurvey(data);
        }

        internal GameObject OnInstance(string sendId, string hash, string group, byte level, Dictionary<byte, object> evData, GameObject go)
        {
            string prefabName = (string)evData[(byte)0];
            InstanceRelation relation = (InstanceRelation)evData[(byte)1];

            //Debug.Log("Instance:" + sendId + "," + hash);
            // 负数是该用户创建的场景物体
            var player = players.GetPlayer(sendId);

            // SetReceiving filtering
            if (group != "" && !allowedReceivingGroups.Contains(group))
            {
                return null; // Ignore group
            }
            Vector3 position = Vector3.zero;
            if (evData.ContainsKey((byte)2))
            {
                var positionBytes = (byte[])evData[(byte)2];
                position = (Vector3)Vector3SerializeFormatter.instance.Deserialize(positionBytes);
            }

            Quaternion rotation = Quaternion.identity;
            if (evData.ContainsKey((byte)3))
            {
                var ratationBytes = (byte[])evData[(byte)3];
                rotation = (Quaternion)QuaternionSerializeFormatter.instance.Deserialize(ratationBytes);
            }

            if (go == null)
            {
                // 统计到该用户的初始化进度中
                players.OnInstance(sendId);
                go = Delegate.OnInstance(prefabName, relation, players.GetPlayer(sendId), position, rotation);
                if (go == null)
                {
                    Debug.LogError("error: Could not Instantiate the prefab [" + prefabName + "]. Please verify you have this gameobject in a Resources folder.");
                    return null;
                }
            }

            NetworkEsse esse = go.GetEsse();
            if (esse == null)
            {
                throw new Exception("Error in Instantiation! The resource's Entity count is not the same as in incoming data.");
            }

            if (evData.ContainsKey((byte)3))
            {
                readStream.SetReadStream((object[])evData[(byte)3], 0);
                esse.UpdateData(readStream);
            }
            esse.group = group;
            esse.level = level;
            esse.isRuntimeInstantiated = true;
            esse.creatorId = sendId;
            // 进入register流程
            esse.Id = hash;

            // 延后执行
            List<object[]> waitData;
            var found = waitInstanceData.TryGetValue(hash, out waitData);
            if (found){
                waitData.ForEach((object[] obj) => esse.OnSerializeRead(readStream, player, obj));
                waitInstanceData.Remove(hash);
            }

            return go;
        }

        private void OnRequest(GamePlayer player, byte code, string key, Dictionary<byte, object> data)
        {
            Delegate.OnRequest(player, code, data, (r) =>
            {
                n.Emit(AreaEvent.Response, new object[] { player.UserId, code, key, MessagePackSerializer.Serialize(r) });
            });
        }

        private void OnResponse(GamePlayer player, byte code, string key, Dictionary<byte, object> data)
        {
            Action<Dictionary<byte, object>> action;
            bool found = requestCache.TryGetValue(key, out action);
            if (found) {
                action(data);
                requestCache.Remove(key);
            } else {
                Debug.Log("Request not exist");
            }

        }

        private void OnAffect(GamePlayer player, Dictionary<byte, object> data)
        {
            
        }

        private void OnCommand(GamePlayer player, Dictionary<byte, object> data)
        {
            string hash = (string)data[(byte)0];
            Debug.Log("Command:" + player + "," + hash);
            NetworkEsse esse = Get(hash);

            if (esse == null)
            {
                Debug.LogWarning("Received OnCommand for ID " + hash + ". We have no such Esse! Ignored this if you're leaving a room. State: " + PeerClient.connected);
                return;
            }

            esse.OnCommand(player, data[(byte)1], data[(byte)2], data[(byte)3]);
        }

        private void OnDestroy(NetworkEsse esse)
        {
            RemoveInstantiated(esse, true);
        }
        #endregion

        //public void DestroyPlayerObjects(int playerId)
        //{
        //    if (playerId <= 0)
        //    {
        //        Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
        //        return;
        //    }

        //    // locally cleaning up that player's objects
        //    HashSet<GameObject> playersGameObjects = new HashSet<GameObject>();
        //    foreach (Dictionary<int, NetworkEntity> list in entityList.Values)
        //    {
        //        foreach (NetworkEntity entity in list.Values)
        //        {
        //            if (entity != null && entity.creatorId == playerId)
        //            {
        //                playersGameObjects.Add(entity.gameObject);
        //            }
        //        }
        //    }

        //    // any non-local work is already done, so with the list of that player's objects, we can clean up (locally only)
        //    foreach (GameObject gameObject in playersGameObjects)
        //    {
        //        RemoveInstantiatedGO(gameObject, true);
        //    }

        //    // with ownership transfer, some objects might lose their owner.
        //    // in that case, the creator becomes the owner again. every client can apply this. done below.
        //    foreach (Dictionary<int, NetworkEntity> list in entityList.Values)
        //    {
        //        foreach (NetworkEntity entity in list.Values)
        //        {
        //            if (entity.ownerId == playerId)
        //            {
        //                entity.ownerId = entity.creatorId;
        //            }
        //        }
        //    }
        //}

        protected void DestroyAll()
        {
            HashSet<NetworkEsse> instantiateds = new HashSet<NetworkEsse>();
            foreach (NetworkEsse esse in esseList.Values)
            {
                if (esse.isRuntimeInstantiated)
                {
                    instantiateds.Add(esse); // HashSet keeps each object only once
                }
            }

            foreach (NetworkEsse esse in instantiateds)
            {
                RemoveInstantiated(esse, true);
            }

            lastUsedViewSubId = 0;
            lastUsedViewSubIdScene = 0;
        }


        protected internal void RemoveInstantiated(NetworkEsse esse, bool localOnly)
        {
            if (esse == null)
            {
                return;
            }
            GameObject go = esse.gameObject;

            string id = esse.Id;

            if (!localOnly)
            {
                if (esse.SyncId == localId)
                {
                    EmitDestroy(esse);
                }
            }

            LocalClean(esse);

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Network destroy Instantiated GO: " + go.name);
            }

            // todo 通知用户
            Delegate.OnDestroyInstance(go, players.GetPlayer(esse.SyncId));
        }

        public static bool LocalClean(NetworkEsse esse)
        {
            esse.removedFromLocalList = true;
            if(esseList.ContainsKey(esse.hash))
            {
                return esseList.Remove(esse.hash);
            }
            else
            {
                return false;
            }
        }

        //
        public static NetworkEsse Get(string hash)
        {
            NetworkEsse result = null;
            esseList.TryGetValue(hash, out result);

            return result;
        }

        public static void Register(NetworkEsse netEsse)
        {
            if (n.State != ConnectionState.Connected)
            {
                return;
            }

            if (netEsse.Id == "")
            {
                Debug.Log("Esse register is ignored, because id is 0. No id assigned yet to: " + netEsse);
                return;
            }

            NetworkEsse esse = Get(netEsse.hash);
            if (esse != null)
            {
                if (netEsse != esse)
                {
                    Debug.LogError(string.Format("Esse ID duplicate found: {0}. New: {1} old: {2}!", netEsse.Id, netEsse, esse));
                }
                else
                {
                    return;
                }

                listener.RemoveInstantiated(esse, true);
            }
            Add(netEsse);

            if (PeerClient.logLevel >= LogLevel.Debug)
            {
                Debug.Log("Registered Esse: " + netEsse.Id);
            }
        }

        private static void Add(NetworkEsse esse)
        {
            if (!esseList.ContainsKey(esse.hash))
            {
                esseList.Add(esse.hash, esse);
            }
        }

        public static int ObjectsInOneUpdate = 50;

        #region sync entity
        internal static void Update()
        {
            if (n.State == ConnectionState.Connected)
            {
                listener.UpdateEsse();
                // todo移除request过期数据
            }
        }

        private void UpdateEsse()
        {
            int countOfUpdatesToSend = 0;

            List<string> toRemove = null;
            var enumerator = esseList.GetEnumerator();
            //Debug.Log("esseList:count:"+ esseList.Count);
            while (enumerator.MoveNext())
            {
                NetworkEsse esse = enumerator.Current.Value;
                if (esse == null)
                {
                    Debug.LogError(string.Format("ID {0} wasn't properly unregistered!", enumerator.Current.Key));

                    if (toRemove == null)
                    {
                        toRemove = new List<string>(4);
                    }
                    toRemove.Add(enumerator.Current.Key);

                    continue;
                }

                // 根据isMine确定同步关系
                if (esse.synchronization == Synchronization.Off || !esse.isMine || esse.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                if (blockSendingGroups.Contains(esse.group))
                {
                    continue; // Block sending on this group
                }

                object[] evData = esse.OnSerializeWrite(pStream, players.GetPlayer(localId));
                //Debug.Log(MessagePackSerializer.Serialize(evData).Length);
                if (evData == null)
                {
                    continue;
                }

                if (esse.synchronization == Synchronization.Reliable || esse.mixedModeIsReliable)
                {
                    Dictionary<byte, List<object[]>> groupHashtable = null;
                    bool found = this.dataPerGroupReliable.TryGetValue(esse.group, out groupHashtable);
                    if (!found)
                    {
                        groupHashtable = new Dictionary<byte, List<object[]>>();
                        this.dataPerGroupReliable[esse.group] = groupHashtable;
                    }
                    List<object[]> levelList = null;
                    found = groupHashtable.TryGetValue(esse.level, out levelList);
                    if (!found)
                    {
                        levelList = new List<object[]>();
                        groupHashtable[esse.level] = levelList;
                    }
                    levelList.Add(evData);
                    //Debug.Log("reliable:"+levelList.Count+esse);
                    if (levelList.Count >= ObjectsInOneUpdate)
                    {
                        countOfUpdatesToSend -= levelList.Count;
                        EmitSerialize(levelList, esse.group, esse.level);
                        levelList.Clear();
                        continue;
                    }
                }
                else
                {
                    Dictionary<byte, List<object[]>> groupHashtable = null;
                    bool found = this.dataPerGroupUnreliable.TryGetValue(esse.group, out groupHashtable);
                    if (!found)
                    {
                        groupHashtable = new Dictionary<byte, List<object[]>>();
                        this.dataPerGroupUnreliable[esse.group] = groupHashtable;
                    }
                    List<object[]> levelList = null;
                    found = groupHashtable.TryGetValue(esse.level, out levelList);
                    if (!found)
                    {
                        levelList = new List<object[]>();
                        groupHashtable[esse.level] = levelList;
                    }
                    levelList.Add(evData);

                    if (levelList.Count >= ObjectsInOneUpdate)
                    {
                        countOfUpdatesToSend -= levelList.Count;
                        EmitSerialize(levelList, esse.group, esse.level);
                        levelList.Clear();
                        continue;
                    }
                }
                countOfUpdatesToSend++;
            }

            if (toRemove != null)
            {
                for (int idx = 0, count = toRemove.Count; idx<count; ++idx)
                {
                    esseList.Remove(toRemove[idx]);
                }
            }

            if (countOfUpdatesToSend == 0)
            {
                return;
            }

            foreach (string groupId in this.dataPerGroupReliable.Keys)
            {
                Dictionary<byte, List<object[]>> groupHashtable = this.dataPerGroupReliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }
                foreach (byte level in groupHashtable.Keys)
                {
                    List<object[]> levelList = groupHashtable[level];
                    if (levelList.Count == 0)
                    {
                        continue;
                    }
                    //Debug.Log("reliable-data:" + levelList.Count);
                    EmitSerialize(levelList, groupId, level);
                    levelList.Clear();
                }
            }

            foreach (string groupId in this.dataPerGroupUnreliable.Keys)
            {
                Dictionary<byte, List<object[]>> groupHashtable = this.dataPerGroupUnreliable[groupId];
                if (groupHashtable.Count == 0)
                {
                    continue;
                }
                foreach (byte level in groupHashtable.Keys)
                {
                    List<object[]> levelList = groupHashtable[level];
                    if (levelList.Count == 0)
                    {
                        continue;
                    }
                    EmitSerialize(levelList, groupId, level);
                    levelList.Clear();
                }
            }
        }

        private void OnSerialize(GamePlayer player, List<object[]> serializeData, string group, byte level)
        {
            var e = serializeData.GetEnumerator();
            while(e.MoveNext())
            {
                var d = e.Current;

                string hash = (string)d[NetworkEsse.SyncHash];
                //Debug.Log("serialize:" + creatorId + ',' + entityId);
                NetworkEsse esse = Get(hash);

                if (esse == null)
                {
                    List<object[]> list;
                    if (!waitInstanceData.TryGetValue(hash, out list))
                    {
                        list = new List<object[]>();
                        waitInstanceData.Add(hash, list);
                    }
                    list.Add(d);
                    Debug.Log("Wait Received Instance:" + ',' + hash);
                    return;
                }

                // SetReceiving filtering
                if (esse.group != "" && !allowedReceivingGroups.Contains(esse.group))
                {
                    return; // Ignore group
                }
                esse.OnSerializeRead(readStream, player, d);
            }
        }

        #endregion

        private void Clear(Action callback)
        {
            //PeerClient.isMessageQueueRunning = false;
            allowedReceivingGroups = new HashSet<string>();
            blockSendingGroups = new HashSet<string>();
            DestroyAll();
            if (callback != null) callback();
        }

        #region Allocate
        internal static string AllocateHash()
        {
            lastUsedViewSubId++;
            var hash = userHash.Encode(lastUsedViewSubId);
            return hash;
        }

        internal static string AllocateSceneHash()
        {
            lastUsedViewSubId++;
            var hash = userHash.Encode(lastUsedViewSubId);
            return hash;
        }
        #endregion

    }
}
