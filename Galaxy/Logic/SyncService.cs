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
		public const byte Area = 1;
        public const byte AcceptGroup = 2;
        public const byte AcceptLevel = 3;
		public const byte Instance = 4;
		public const byte ChangeInfo = 5;
		public const byte ChangeLocation = 6;
		public const byte Destroy = 7;
		public const byte Serialize = 8;
		public const byte UpdateData = 9;
		public const byte Request = 10;
		public const byte Response = 11;
		public const byte Ownership = 12;
		public const byte Belong = 13;
		public const byte Callback = 14;
		public const byte Broadcast = 15;
		public const byte Assign = 16;
    }

    internal class SyncEvent
    {
        public const byte Command = 1;
    }

    public interface AreaListener
    {
        GameObject OnInstance(string prefabName, byte relation, GamePlayer player, Vector3 position, Quaternion rotatio, bool isLocal);
        void OnDestroyInstance(GameObject gameObject, GamePlayer player);
        void OnRequest(GamePlayer player, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback);
    }
    public interface PlayerFactory
    {
        GamePlayer GetPlayer(string userId);
        void OnInstance(string userId);
    }
    public class AreaDelegate : AreaListener
    {
        public delegate GameObject OnInstanceDelegate(string prefabName, byte relation, GamePlayer player, Vector3 position, Quaternion rotation, bool isLocal);
        public delegate void OnDestroyInstanceDelegate(GameObject gameObject, GamePlayer player);
        public delegate void OnRequestDelegate(GamePlayer player, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback);

        public OnInstanceDelegate OnInstanceEvent;
        public OnDestroyInstanceDelegate OnDestroyInstanceEvent;
        public OnRequestDelegate OnRequestEvent;

        internal void Bind(AreaListener listener)
        {
            OnInstanceEvent = listener.OnInstance;
            OnDestroyInstanceEvent = listener.OnDestroyInstance;
            OnRequestEvent = listener.OnRequest;
        }

        public GameObject OnInstance(string prefabName, byte relation, GamePlayer player, Vector3 position, Quaternion rotation, bool isLocal)
        {
			if (OnInstanceEvent != null) return OnInstanceEvent(prefabName, relation, player, position, rotation, isLocal);
            return null;
        }

        public void OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            if (OnDestroyInstanceEvent != null) OnDestroyInstanceEvent(gameObject, player);
        }

        public void OnRequest(GamePlayer player, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback)
        {
            if (OnRequestEvent != null) OnRequestEvent(player, value, callback);
        }
    }

	internal class Callback
	{
		internal byte e;
		internal Action<object[]> action;
	}

	public class SyncService : NetworkListener,ServiceListener
    {
		internal static AreaDelegate Delegate;
        private static CometProxy proxy;
		private static SyncService listener;
		public static SyncService Listener
        {
            get
            {
                return listener;
            }
        }
		private static string area = "";
		private static bool online = false;
        private Action<bool> OnConnectAction;

        private PlayerFactory players;

        protected static internal Dictionary<string, NetworkEsse> esseList = new Dictionary<string, NetworkEsse>();

        internal static HashidsNet.Hashids userHash = null;
        internal static string localId = "";
        internal static int lastUsedViewSubId = 0;
        internal static int lastHashKeyId = 0;

        private static HashSet<string> allowedReceivingGroups = new HashSet<string>();
        private static HashSet<string> blockSendingGroups = new HashSet<string>();
        
        private readonly StreamBuffer readStream = new StreamBuffer(false, null);    // only used in OnSerializeRead()
        private readonly StreamBuffer pStream = new StreamBuffer(true, null);        // only used in OnSerializeWrite()
        private readonly Dictionary<string, Dictionary<byte, List<object[]>>> dataPerGroupReliable = new Dictionary<string, Dictionary<byte, List<object[]>>>();    // only used in RunViewUpdate()
        private readonly Dictionary<string, Dictionary<byte, List<object[]>>> dataPerGroupUnreliable = new Dictionary<string, Dictionary<byte, List<object[]>>>();  // only used in RunViewUpdate()

        private readonly Dictionary<string, Action<Dictionary<byte, object>>> requestCache = new Dictionary<string, Action<Dictionary<byte, object>>>();
        private readonly Dictionary<string, List<object[]>> waitInstanceData = new Dictionary<string, List<object[]>>();

		private readonly Dictionary<string, Action<string>> ownershipCallbackCache = new Dictionary<string, Action<string>>();
		private readonly Dictionary<string, Action<string>> belongCallbackCache = new Dictionary<string, Action<string>>();
		private readonly Dictionary<string, Action> broadcastCallbackCache = new Dictionary<string, Action>();

		static SyncService()
        {
            listener = new SyncService();
			proxy = PeerClient.Register(1, listener);
            Delegate = new AreaDelegate();
			listener.OnConnectEvent += (bool success) => {
				proxy.Send(AreaEvent.Area, area.GetBytes());
				if (listener.OnConnectAction != null) listener.OnConnectAction(success);
			};
        }

        public static void Join(string token, string userId, PlayerFactory factory, bool o, Action<bool> action)
        {
            localId = userId;
            listener.players = factory;
            userHash = new HashidsNet.Hashids(token + localId);

			area = token;
			online = !PeerClient.offlineMode || online;
			if (!online){
                action(true);
            } else {
                listener.OnConnectAction = action;
				proxy.Join();
            }
		}

        public static void Leave()
        {
			area = "";
            proxy.Leave();
        }

        public static void StartInit(AreaListener listener)
        {
            Delegate.Bind(listener);
            Listener.ResetOnSerialize();
        }

        public static int FinishInit()
        {
            return lastUsedViewSubId;
        }

        public void Update()
        {
            if (proxy.IsConnected)
            {
                listener.UpdateEsse();
                // todo移除request过期数据
            }
        }

        public void OnAccept(byte de, Message message)
        {
			object[] param = null;
			switch (de)
            {
                case AreaEvent.Instance:
					// group string, level byte, hash string, info []byte, dataLength, ownerId string, belong string, assign string
					param = message.ReadObject();
					proxy.AcceptQueue(de, param);
					break;
                case AreaEvent.ChangeInfo:               
                    // group string, level byte, hash string, info []byte
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
                case AreaEvent.ChangeLocation:
					// group string, level byte, hash string, newGroup string, newLevel byte
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
                case AreaEvent.Serialize:
					// group, level, value
					param = message.ReadObject();               
                    proxy.AcceptQueue(de, param);
					break;
				case AreaEvent.UpdateData:
					// group, level, hash, index, data
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
					break;
				case AreaEvent.Broadcast:
                    // group, level, code, value, sendId
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
				case AreaEvent.Ownership:
                    // group string, level byte, hash string, release bool, ownerId
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
				case AreaEvent.Belong:
                    // group string, level byte, hash string, release bool, belong
                    param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
					break;
                case AreaEvent.Destroy:
					// group string, level byte, hash string
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
                case AreaEvent.Request:
                    // userId, key, data
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
                case AreaEvent.Response:
                    // userId, key, data
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
                    break;
				case AreaEvent.Callback:
					// event, key, value
					param = message.ReadObject();
                    proxy.AcceptQueue(de, param);
					break;
				case AreaEvent.Assign:
					// group string, level byte, hash string, assignId
					param = message.ReadObject();               
                    proxy.AcceptQueue(de, param);
					break;
                default:
                    Debug.Log("AreaEvent is error:" + de);
                    break;
            }
            return;
		}

		public void OnAcceptQueue(byte de, object[] param)
        {
            switch (de)
            {
                case AreaEvent.Instance:
                    // group string, level byte, hash string, info []byte, data []object, ownerId string, belong string, assign string
                    listener.OnInstance((string)param[0], (byte)param[1], (string)param[2], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[3]), (object[])param[4],(string)param[5], (string)param[6], (string)param[7], null);
                    break;
                case AreaEvent.ChangeInfo:
					// group string, level byte, hash string, info []byte
					listener.OnChangeInfo(Get((string)param[2]), MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[3]));
                    break;
                case AreaEvent.ChangeLocation:
                    // group string, level byte, hash string, newGroup string, newLevel byte
                    break;
                case AreaEvent.Serialize:
                    // group, level, value
                    listener.OnSerialize((string)param[0], (byte)param[1], MessagePackSerializer.Deserialize<List<object[]>>((byte[])param[2]));
                    break;
                case AreaEvent.UpdateData:
                    // group, level, hash, index, data
                    listener.OnUpdateData(Get((string)param[2]), (byte)param[3], param[4]);
                    break;
                case AreaEvent.Broadcast:
                    // group, level, code, value, sendId
                    listener.OnBroadcast((string)param[0], (byte)param[1], (byte)param[2], (byte[])param[3], (string)param[4]);
                    break;
                case AreaEvent.Ownership:
                    // group string, level byte, hash string, release bool, ownerId
                    listener.OnOwnership(Get((string)param[2]), (string)param[4]);
                    break;
                case AreaEvent.Belong:
                    // group string, level byte, hash string, release bool, belong
                    listener.OnBelong(Get((string)param[2]), Get((string)param[4]));
                    break;
                case AreaEvent.Destroy:
                    // group string, level byte, hash string
                    listener.OnDestroy(Get((string)param[2]));
                    break;
                case AreaEvent.Request:
                    // userId, key, data
                    listener.OnRequest(players.GetPlayer((string)param[0]), (string)param[1], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[2]));
                    break;
                case AreaEvent.Response:
                    // userId, key, data
                    listener.OnResponse(players.GetPlayer((string)param[0]), (string)param[1], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[2]));
                    break;
                case AreaEvent.Callback:
                    // event, key, value
                    listener.OnCallback((byte)param[0], (string)param[1], (byte[])param[2]);
                    break;
                case AreaEvent.Assign:
                    // group string, level byte, hash string, assignId
                    listener.OnAssign(Get((string)param[2]), players.GetPlayer((string)param[3]));
                    break;
                default:
                    Debug.Log("AreaEvent is error:" + de);
                    break;
            }
            return;
        }

		private void OnBroadcast(string group, byte level, byte code, byte[] value, string sendId)
        {
            GamePlayer player = players.GetPlayer(sendId);
            switch (code)
            {
                case SyncEvent.Command:
                    listener.OnCommand(player, MessagePackSerializer.Deserialize<Dictionary<byte, object>>(value));
                    break;
                default:
                    Debug.Log("Sync event is error:");
                    break;
            }
        }

        private void OnCallback(byte code, string key, byte[] value)
		{
			bool result;
			switch(code)
			{
				case AreaEvent.Ownership:
					Action<string> ownershipCallback;
					result = ownershipCallbackCache.TryGetValue(key, out ownershipCallback);
                    if (result)
					{
						ownershipCallback(value.GetString());
					}
					break;
				case AreaEvent.Belong:
					Action<string> belongCallback;
					result = belongCallbackCache.TryGetValue(key, out belongCallback);
                    if (result)
					{
						belongCallback(value.GetString());
					}
					break;
				case AreaEvent.Broadcast:
					Action broadcastCallback;
					result = broadcastCallbackCache.TryGetValue(key, out broadcastCallback);
					if (result)
					{
						broadcastCallback();
					}
					break;
			}
		}

        private static void Emit(byte code, NetworkEsse esse)
        {
			proxy.SendObject(code, new object[] { esse.group, esse.level, esse.hash });
        }

		private static void Emit(byte code, NetworkEsse esse, object value)
        {
			proxy.SendObject(code, new object[] { esse.group, esse.level, esse.hash, value });
        }
		private static void Emit(byte code, NetworkEsse esse, object value1, object value2)
        {
			proxy.SendObject(code, new object[] { esse.group, esse.level, esse.hash, value1, value2 });
        }

		private static void EmitBroadcast(byte code, Dictionary<byte, object> value, string group, byte level, Action callback=null)
        {
			string key = null;
			if (callback != null)
			{
				key = AllocateHash();
                listener.broadcastCallbackCache[key] = callback;
			}
			proxy.SendObject(AreaEvent.Broadcast, new object[] { group, level, code, MessagePackSerializer.Serialize(value), key });
        }

        private static void EmitSerialize(List<object[]> value, string group, byte level)
        {
            byte[] info = MessagePackSerializer.Serialize(value);
			proxy.SendObject(AreaEvent.Serialize, new object[] { group, level, info });
		}
#region Emit event
		public static void SetGroups(string[] disableGroups, string[] enableGroups)
        {
            if (online) {
                proxy.SendObject(AreaEvent.AcceptGroup, new object[] { disableGroups, enableGroups });
            }
        }
        public static void SetLevel(byte[] disableLevel, byte[] enableLevel)
        {
            if (online) {
                proxy.SendObject(AreaEvent.AcceptLevel, new object[] { disableLevel, enableLevel });
            }
        }
        public static void ChangeInfo(NetworkEsse esse)
        {
			if (!esse.isMine) return;
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

            var info = esse.GetInfo(listener.pStream);

			if (info)
            {
                instantiateEvent[(byte)3] = listener.pStream.ToArray();
            }

            proxy.SendObject(AreaEvent.ChangeInfo, new object[] { esse.hash, esse.group, esse.level, MessagePackSerializer.Serialize(instantiateEvent) });
        }
        public static void ChangeLocation(NetworkEsse esse, string group, byte level)
		{
            if (!esse.isMine) return;
            if (online) {            
                proxy.SendObject(AreaEvent.ChangeLocation, new object[] { esse.hash, esse.group, esse.level, group, level });
            }
            esse.group = group;
            esse.level = level;
        }    

		public static GameObject Instance(string prefabName, byte relation, Vector3 position, Quaternion rotation, bool isOwner, GameObject master, byte dataLength)
        {
			NetworkEsse masterEsse = null;
			if (master != null)
			{
				masterEsse = master.GetComponent<NetworkEsse>() as NetworkEsse;
				if (!masterEsse.isMine) return null;
			}
			var assign = listener.players.GetPlayer(localId);
            var hash = AllocateHash();
            var ownerId = isOwner ? localId : "";
			var data = new object[dataLength];
            
            var gg = Delegate.OnInstance(prefabName, relation, assign, position, rotation, true);
			var esse = gg.GetEsse();
			esse.belong = masterEsse;

			Dictionary<byte, object> instantiateEvent = listener.EmitInstantiate(esse, prefabName, relation, gg, ownerId,  dataLength);
			//group string, level byte, hash string, info []byte, ownerId string, belong string, assign string
			listener.OnInstance(esse.group, esse.level, hash, instantiateEvent, data, ownerId, esse.belong != null ? esse.belong.hash : null, esse.assign.UserId, gg);
			return gg;
        }

        public static void Destroy(NetworkEsse esse)
        {
            if (!esse.isMine) return;
            if (!online) {
                listener.RemoveInstantiated(esse, true);
			} else if (!proxy.IsConnected){
                Debug.LogError("Failed to Destroy Entity");
                return;
            } else {
                listener.RemoveInstantiated(esse, false);
            }
		}

        public static object UpdateData(NetworkEsse esse, byte index, object data)
		{
            if (!esse.isMine) return esse.data[index];
			esse.data[index] = data;
			if(online)
			{
				// todo 队列，统一更新，按频次，没有
				Emit(AreaEvent.UpdateData, esse, index, data);
			}
			return esse.data[index];
		}

        public static void Ownership(NetworkEsse esse, string want, Action<string> callback)
        {
			if (!esse.isMine)
			{
				callback(esse.owner.UserId);
			}
			else if (online)
            {
                if (listener.ownershipCallbackCache.ContainsKey(esse.hash)) return;
				listener.ownershipCallbackCache[esse.hash] = (b)=>{
					esse.owner = listener.players.GetPlayer(b);
					callback(b);
				};
				Emit(AreaEvent.Ownership, esse, want);
            }
            else
            {
				esse.owner = listener.players.GetPlayer(want);
				callback(want);
            }
        }

        public static void Belong(NetworkEsse esse, NetworkEsse belong, Action<string> callback)
		{
			if (!esse.isMine)
            {
				callback(esse.belong.hash);
            }
			else if (online)
            {
				if (listener.belongCallbackCache.ContainsKey(esse.hash)) return;
				listener.belongCallbackCache[esse.hash] = (b)=>{
					esse.belong = Get(b);
					callback(b);
				};
				Emit(AreaEvent.Belong, esse, belong != null ? belong.hash : "");
            }
            else
            {
				callback(belong.hash);
            }
		}

        public static void Command(NetworkEsse esse, Action callback, Dictionary<byte, object> data)
        {
			if (!esse.isMine)
            {
                callback();
            }
            else if (!online) {
                callback();
                return;
			} else if (!proxy.IsConnected){
                Debug.LogError("Failed to Send Command");
                return;
            }
            data[(byte)254] = esse.hash;
			EmitBroadcast(SyncEvent.Command, data, esse.group, esse.level, callback);
        }

        public static void Request(GamePlayer player, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback)
        {
			var key = AllocateHashKey();
            listener.requestCache[key] = callback;
			proxy.SendObject(AreaEvent.Request, new object[] { player.UserId, key, MessagePackSerializer.Serialize(value) });
        }
#endregion

		internal Dictionary<byte, object> EmitInstantiate(NetworkEsse esse, string prefabName, byte relation, GameObject go, string ownerId, byte dataLength)
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

            var data = esse.GetInfo(pStream);

            if (data)
            {
                instantiateEvent[(byte)4] = pStream.ToArray();
            }
            if (online)
				proxy.SendObject(AreaEvent.Instance, new object[] { esse.group, esse.level, esse.hash, MessagePackSerializer.Serialize(instantiateEvent), dataLength, ownerId, esse.belong });
            return instantiateEvent;
        }

        private void EmitDestroy(NetworkEsse esse)
        {
            Emit(AreaEvent.Destroy, esse, null);
        }

        #region OnEvent

		internal GameObject OnInstance(string group, byte level, string hash, Dictionary<byte, object> evData, object[]data ,string ownerId, string belong, string assign, GameObject go)
        {
            string prefabName = (string)evData[(byte)0];
            byte relation = (byte)evData[(byte)1];

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
                players.OnInstance(assign);
                go = Delegate.OnInstance(prefabName, relation, players.GetPlayer(assign), position, rotation, false);
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
				esse.InitInfo(readStream, position, rotation);
            }
            esse.group = group;
            esse.level = level;
			esse.belong = Get(belong);
			esse.assign = players.GetPlayer(assign);
            esse.isRuntimeInstantiated = true;
            esse.owner = players.GetPlayer(ownerId);
            // 进入register流程
            esse.Id = hash;

			// 跟新data
			esse.data = data;

            // 延后执行
            List<object[]> waitData;
            var found = waitInstanceData.TryGetValue(hash, out waitData);
            if (found){
                waitData.ForEach((object[] obj) => esse.OnSerializeRead(readStream, obj));
                waitInstanceData.Remove(hash);
            }

            return go;
		}

		private void OnChangeInfo(NetworkEsse esse, Dictionary<byte, object> evData)
		{
			
		}

        private void OnUpdateData(NetworkEsse esse, byte index, object data)
		{
			if (esse == null) return;
			esse.OnUpdateData(index, data);
		}

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

		private void OnBelong(NetworkEsse esse, NetworkEsse belong)
        {
			if (esse == null) return;

			esse.OnBelong(belong);
		}

        private void OnDestroy(NetworkEsse esse)
        {
            RemoveInstantiated(esse, true);
        }

        private void OnRequest(GamePlayer player, string key, Dictionary<byte, object> data)
        {
            Delegate.OnRequest(player, data, (r) =>
			{
				proxy.SendObject(AreaEvent.Response, new object[] { player.UserId, key, MessagePackSerializer.Serialize(r) });
            });
        }

        private void OnResponse(GamePlayer player, string key, Dictionary<byte, object> data)
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

        private void OnCommand(GamePlayer player, Dictionary<byte, object> data)
        {
            var hash = (string)data[(byte)254];
            NetworkEsse esse = Get(hash);

            if (esse == null)
            {
                Debug.LogWarning("Received OnCommand for ID " + hash + ". We have no such Esse! Ignored this if you're leaving a room. State: " + PeerClient.connected);
                return;
            }

            esse.OnCommand(player, data);
        }
		private void OnAssign(NetworkEsse esse, GamePlayer player)
		{
			esse.OnAssign(player);
		}
		#endregion

#region Esse
		private void ResetOnSerialize()
        {
            foreach (NetworkEsse esse in esseList.Values)
            {
                esse.lastOnSerializeDataSent = null;
            }
        }
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
			if (proxy.IsConnected)
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
#endregion

		#region sync entity

		public static int ObjectsInOneUpdate = 50;
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

                object[] evData = esse.OnSerializeWrite(pStream);
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

		private void OnSerialize(string group, byte level, List<object[]> serializeData)
        {
			// SetReceiving filtering
            if (!allowedReceivingGroups.Contains(group))
            {
                return; // Ignore group
			}
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

               
                esse.OnSerializeRead(readStream, d);
            }
        }

        #endregion

        private void Clear(Action callback)
        {
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
		internal static string AllocateHashKey()
        {
			lastHashKeyId++;
			var hash = userHash.Encode(lastHashKeyId);
            return hash;
        } 
		#endregion
	}
}
