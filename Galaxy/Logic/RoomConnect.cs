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
        public const byte Join = 0;
        public const byte Leave = 1;
        public const byte Ready = 2;
        public const byte ReadyAll = 3;
        public const byte Broadcast = 4;
        public const byte Exit = 5;
        public const byte Finish = 6;
        public const byte Area = 7;
    }
    internal class BroadcastEvent
    {
        public const byte Init = 0;
    }

    public enum GameStage
    {
        None,
        Connect,
        Wait,
        Join,
        Ready,
        Init,
        Start,
        Finish
    }

    public interface GameBeforeListener
    {
        void OnEnter(bool success);
        void OnPlayerJoin(GamePlayer player);
        void OnPlayerLeave(GamePlayer player);
        void OnRoomChange(Dictionary<string, object> props);
        void OnLeaveGame();
    }

    public interface GameReadyListener
    {
        void OnSync(bool success);
        void OnStart();
        void OnLeaveGame();
        void OnPlayerLeave(GamePlayer player);
        void OnPlayerRejoin(GamePlayer player);
        void OnPlayerChange(GamePlayer player, Dictionary<string, object> props);
        void OnReadyPlayer(GamePlayer player);
        void OnReadyAll();
    }

    public interface GameListener: AreaListener
    {
        void OnFinish(Dictionary<string, object> result);
        void OnLeaveGame();
        void OnFail(string reason);
        void OnPlayerLeave(GamePlayer player);
        void OnPlayerRejoin(GamePlayer player);
        void OnPlayerChange(GamePlayer player, Dictionary<string, object> props);
    }

    public class GameDelegate : AreaDelegate, GameBeforeListener, GameReadyListener, GameListener
    {
        public delegate void OnFinishDelegate(Dictionary<string, object> result);
        public delegate void OnStartDelegate();
        public delegate void OnLeaveGameDelegate();

        public delegate void OnEnterDelegate(bool success);
        public delegate void OnFailDelegate(string reason);
        public delegate void OnSyncDelegate(bool success);
        public delegate void OnRoomChangeDelegate(Dictionary<string, object> props);
        public delegate void OnPlayerJoinDelegate(GamePlayer player);
        public delegate void OnPlayerLeaveDelegate(GamePlayer player);
        public delegate void OnPlayerRejoinDelegate(GamePlayer player);
        public delegate void OnPlayerChangeDelegate(GamePlayer player, Dictionary<string, object> props);
        public delegate void OnReadyPlayerDelegate(GamePlayer player);
        public delegate void OnReadyAllDelegate();

        public OnEnterDelegate OnEnterEvent;
        public OnFailDelegate OnFailEvent;
        public OnSyncDelegate OnSyncEvent;
        public OnRoomChangeDelegate OnRoomChangeEvent;
        public OnPlayerChangeDelegate OnPlayerChangeEvent;
        public OnPlayerJoinDelegate OnPlayerJoinEvent;
        public OnPlayerLeaveDelegate OnPlayerLeaveEvent;
        public OnPlayerRejoinDelegate OnPlayerRejoinEvent;
        public OnReadyPlayerDelegate OnReadyPlayerEvent;
        public OnReadyAllDelegate OnReadyAllEvent;
        public OnFinishDelegate OnFinishEvent;
        public OnStartDelegate OnStartEvent;
        public OnLeaveGameDelegate OnLeaveGameEvent;

        internal void BindBefore(GameBeforeListener listener)
        {
            OnPlayerJoinEvent = listener.OnPlayerJoin;
            OnPlayerLeaveEvent = listener.OnPlayerLeave;
            OnRoomChangeEvent = listener.OnRoomChange;
            OnLeaveGameEvent = listener.OnLeaveGame;
            OnEnterEvent = listener.OnEnter;
        }

        internal void BindReady(GameReadyListener listener)
        {
            OnSyncEvent = listener.OnSync;
            OnStartEvent = listener.OnStart;
            OnLeaveGameEvent = listener.OnLeaveGame;
            OnPlayerLeaveEvent = listener.OnPlayerLeave;
            OnPlayerRejoinEvent = listener.OnPlayerRejoin;
            OnPlayerChangeEvent = listener.OnPlayerChange;
            OnReadyPlayerEvent = listener.OnReadyPlayer;
            OnReadyAllEvent = listener.OnReadyAll;
        }

        internal void BindGame(GameListener listener)
        {
            OnFinishEvent = listener.OnFinish;
            OnLeaveGameEvent = listener.OnLeaveGame;
            OnFailEvent = listener.OnFail;
            OnPlayerLeaveEvent = listener.OnPlayerLeave;
            OnPlayerRejoinEvent = listener.OnPlayerRejoin;
            OnPlayerChangeEvent = listener.OnPlayerChange;
        }

        public void OnFinish(Dictionary<string, object> result)
        {
            if (OnFinishEvent != null) OnFinishEvent(result);
        }

        public void OnStart()
        {
            if (OnStartEvent != null) OnStartEvent();
        }

        public void OnLeaveGame()
        {
            if (OnLeaveGameEvent != null) OnLeaveGameEvent();
        }

        public void OnEnter(bool success)
        {
            if (OnEnterEvent != null) OnEnterEvent(success);
        }

        public void OnFail(string reason)
        {
            if (OnFailEvent != null) OnFailEvent(reason);
        }

        public void OnSync(bool success)
        {
            if (OnSyncEvent != null) OnSyncEvent(success);
        }

        public void OnRoomChange(Dictionary<string, object> props)
        {
            if (OnRoomChangeEvent != null) OnRoomChangeEvent(props);
        }

        public void OnPlayerJoin(GamePlayer player)
        {
            if (OnPlayerJoinEvent != null) OnPlayerJoinEvent(player);
        }

        public void OnPlayerLeave(GamePlayer player)
        {
            if (OnPlayerLeaveEvent != null) OnPlayerLeaveEvent(player);
        }

        public void OnPlayerRejoin(GamePlayer player)
        {
            if (OnPlayerRejoinEvent != null) OnPlayerRejoinEvent(player);
        }

        public void OnPlayerChange(GamePlayer player, Dictionary<string, object> props)
        {
            if (OnPlayerChangeEvent != null) OnPlayerChangeEvent(player, props);
        }

        public void OnReadyPlayer(GamePlayer player)
        {
            if (OnReadyPlayerEvent != null) OnReadyPlayerEvent(player);
        }

        public void OnReadyAll()
        {
            if (OnReadyAllEvent != null) OnReadyAllEvent();
        }
    }

    public class RoomConnect : NamespaceListener
    {
        internal static GameDelegate Delegate;
        private static Namespace n;
        public static GameRoom Room;
        private static string token;
        private static string areaToken;
        private static RoomConnect listener;
        public static GameStage stage;

        public static bool inRoom
        {
            get
            {
                return Room != null;
            }
        }

        static RoomConnect()
        {
            n = SceneConnect.Of(SceneRoom.Room);
            //n.compress = CompressType.Snappy;
            //n.protocol = ProtocolType.Speed;
            //n.messageQueue = MessageQueue.On;
            listener = new RoomConnect();
            n.listener = listener;
            Delegate = new GameDelegate();
            Room = new GameRoom(Delegate);
        }
        #region flow
        // SceneConnect收到游戏房间后，执行链接游戏
        public static void ConnectGame(string gameName, GameBeforeListener listener)
        {
            if (stage != GameStage.None)
                throw new Exception("Game is going");
            if (listener == null)
                throw new Exception("Game Before Delegate empty");
            // 需要设定玩家委托
            // 会触发游戏进入成功，开始接受相关房间事件
            // 玩家加入，玩家修改信息
            //PeerClient.isMessageQueueRunning = true;
            Delegate.BindBefore(listener);
            token = gameName;
            stage = GameStage.Connect;

            if (PeerClient.offlineMode) {
                listener.OnEnter(true);
            } else {
                n.Connect("token=" + token);
            }
        }

        // GameConnect所有玩家Connect后操作，执行进入游戏
        public static void JoinGame(GameReadyListener listener)
        {
            if (listener == null)
                throw new Exception("Game Ready Delegate empty");
            if (!PeerClient.offlineMode && stage != GameStage.Wait)
                throw new Exception("game stage need wait");
            Delegate.BindReady(listener);
            stage = GameStage.Join;

            //if (PeerClient.offlineMode) {
                
            //} else {
            //}
            AreaConnect.Join(areaToken, Room.LocalId, Room, listener.OnSync);
        }

        // 通知服务器已经能开始游戏
        public static void ReadyGame()
        {
            // 会发送给其他玩家准备完毕事件
            // 会触发所有玩家准备完毕事件
            if (stage != GameStage.Join)
                throw new Exception("game stage need join");
            Room.localPlayer.IsReady = true;
            stage = GameStage.Ready;

            if (PeerClient.offlineMode) {
                listener.OnEvent(GameEvent.ReadyAll, null);
            } else {
                n.Emit(GameEvent.Ready, null);
            }
        }

        //开始进行场景同步，需要已经进入必要的scene中
        public static void StartInitGame(GameListener gameListener)
        {
            if (gameListener == null)
                throw new Exception("Game Delegate empty");
            if (!Room.localPlayer.IsReady)
                throw new Exception("local player need ready");
            if (stage != GameStage.Ready && stage != GameStage.Init)
                throw new Exception("game stage need join");
            Delegate.BindGame(gameListener);
            //可以开始进行游戏初始化
            stage = GameStage.Init;
            //if (PeerClient.offlineMode) {
                
            //} else {
            //}
            AreaConnect.StartInit(gameListener);
        }

        //完成基础初始化，触发开始游戏
        public static void FinishInitGame()
        {
            if (!Room.localPlayer.IsReady)
                throw new Exception("local player need ready");
            if (stage != GameStage.Init)
                throw new Exception("game stage need start");

            int number = AreaConnect.FinishInit();
            Room.OnInit(Room.LocalId, number);

            if (PeerClient.offlineMode) {
                stage = GameStage.Start;
                Delegate.OnStart();
            } else {
                var data = new Dictionary<byte, object>();
                data[0] = number;
                EmitBroadcastCallback(BroadcastEvent.Init, data, (object[] obj) => {
                    stage = GameStage.Start;
                    Delegate.OnStart();
                });
            }
        }

        public static void FinishGame()
        {
            if (stage != GameStage.Start)
                throw new Exception("game stage need start");
            if (PeerClient.offlineMode) {
                
            } else {
                n.Emit(GameEvent.Finish, null);
                AreaConnect.Leave();
            }
        }

        // 准备退出游戏
        public static void LeaveGame(Action callback)
        {
            if (stage == GameStage.None)
                throw new Exception("need Join Room first");
            if (PeerClient.offlineMode)
            {
                listener.Clear(callback);
                return;
            }
            // 如果游戏已经完成直接退出
            if (stage == GameStage.Finish)
            {
                n.Disconnect();
                AreaConnect.Leave();
                listener.Clear(callback);
            }
            // 如果游戏还未开始，执行退出后直接退出
            else
            {
                n.Emit(GameEvent.Exit, null, (obj) =>
                {
                    n.Disconnect();
                    AreaConnect.Leave();
                    listener.Clear(callback);
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
                Delegate.OnEnter(false);
            }
        }

        public void OnDisconnect()
        {
            Delegate.OnLeaveGame();
            Clear(null);
        }

        public void OnReconnect(bool success)
        {
            if (!success)
            {
                Delegate.OnFail("connect is fail");
            }
        }

        public void OnError(string message)
        {
            Debug.Log(message);
        }

        public object[] OnEvent(byte code, object[] param)
        {
            if (stage == GameStage.None)
            {
                Debug.Log("connect is error, need in room");
                return null;
            }
            switch (code)
            {
                case GameEvent.Join:
                    Room.OnJoin((string)param[0]);
                    //active
                    if (!(bool)param[1])
                    {
                        Room.OnLeave((string)param[0]);
                    }
                    //ready
                    if ((bool)param[2])
                    {
                        Room.OnReady((string)param[0]);
                    }
                    break;
                case GameEvent.Leave:
                    Room.OnLeave((string)param[0]);
                    break;
                case GameEvent.Area:
                    areaToken = (string)param[0];
                    stage = GameStage.Wait;
                    Delegate.OnEnter(true);
                    break;
                case GameEvent.Ready:
                    Room.OnReady((string)param[0]);
                    break;
                case GameEvent.ReadyAll:
                    stage = GameStage.Init;
                    Room.OnReadyAll();
                    break;
                case GameEvent.Broadcast:
                    OnBroadcast((byte)param[0],(string)param[1], MessagePackSerializer.Deserialize<Dictionary<byte, object>>((byte[])param[2]));
                    break;
                case GameEvent.Finish:
                    stage = GameStage.Finish;
                    Delegate.OnFinish(((Dictionary<object, object>)param[0]).ConverString());
                    AreaConnect.Leave();
                    break;
                default:
                    Debug.Log("GameEvent is error:" + code);
                    break;
            }
            return null;
        }

        private static void EmitBroadcastCallback(byte code, Dictionary<byte, object> value, Action<object[]> callback = null)
        {
            n.Emit(GameEvent.Broadcast, new object[] { code, MessagePackSerializer.Serialize(value) }, callback);
        }


        #region OnEvent

        private void OnBroadcast(byte code, string sendId, Dictionary<byte, object> param)
        {
            switch (code)
            {
                case BroadcastEvent.Init:
                    Room.OnInit(sendId, param[0].ConverInt());
                    break;
                default:
                    Debug.Log("Broadcast event is error:");
                    break;
            }
        }

        #endregion


        private void Clear(Action callback)
        {
            stage = GameStage.None;
            //PeerClient.isMessageQueueRunning = false;
            Room.Clear();
            if(callback != null) callback();
        }

    }
}
