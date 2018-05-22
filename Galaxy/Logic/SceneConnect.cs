using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic
{

    internal class SceneEvent
    {
        public const byte LobbyJoin = 0;
        public const byte LobbyLeave = 1;
        public const byte LobbyExist = 2;
        public const byte TeamCreate = 3;
        public const byte ChangeProp = 4;
        public const byte UserProp = 5;
        public const byte TeamInvite = 254;
        public const byte GameConnect = 253;
        public const byte Prop = 252;
    }
    public class LobbyType
    {
        public const string PVE = "pve";
        public const string PVP = "pvp";
    }
    // player 信息
    // lobby 信息
    // team和game接入信息
    public class SceneConnect : NetworkListener, NamespaceListener
    {
        private static Namespace n;
        private static SceneConnect listener;
        public static SceneConnect Listener
        {
            get
            {
                return listener;
            }
        }

        public delegate void OnJoinedGameDelegate(string token);
        public delegate void OnInvitedTeamDelegate(string userId, string teamId);
        public delegate void OnPlayerInitDelegate(GamePlayer player);

        public OnJoinedGameDelegate OnJoinedGame;
        public OnInvitedTeamDelegate OnInvitedTeam;
        public OnPlayerInitDelegate OnPlayerInit;

        private Action<bool> OnConnectAction;

        public static GamePlayer player = new GamePlayer(true, "");

        static SceneConnect()
        {
            n = PeerClient.Of(NamespaceId.Scene);
            listener = new SceneConnect();
            n.listener = listener;
            listener.OnConnectEvent += (success) =>
            {
                if (listener.OnConnectAction != null) listener.OnConnectAction(success);
            };
        }

        public static void Connect(Action<bool> a)
        {
            Listener.OnConnectAction = a;
            n.Connect();
        }

        public static void Disconnect()
        {
            n.Disconnect();
        }

        public static void JoinLobby(string lobby, Dictionary<string, object> options)
        {
            switch (lobby)
            {
                default:
                    throw new Exception("lobby type is empty");
                case LobbyType.PVE:
                case LobbyType.PVP:
                    break;
            }
            n.Emit(SceneEvent.LobbyJoin, new object[] { lobby, options });
        }

        public static void CreateTeam(int people, Action<string> callback)
        {
            n.Emit(SceneEvent.TeamCreate, new object[] { people }, (obj) => callback((string)obj[0]));
        }

        public static void LeaveLobby()
        {
            n.Emit(SceneEvent.LobbyLeave, new object[] { });
        }

        public static void ExistLobby(Action<bool> callback)
        {
            n.Emit(SceneEvent.LobbyExist, new object[] { }, (obj) => callback((bool)obj[0]));
        }

        public static void SetProperties(Dictionary<string, object> customProperties)
        {
            if (customProperties == null)
            {
                customProperties = new Dictionary<string, object>();
                foreach (object k in player.Properties.Keys)
                {
                    customProperties[(string)k] = null;
                }
            }
            player.SetProperties(customProperties);
            emitPlayerProp(player.Properties);
        }
        public static void RemoveProperties(string[] customPropertiesToDelete)
        {
            var props = player.Properties;
            for (int i = 0; i < customPropertiesToDelete.Length; i++)
            {
                string key = customPropertiesToDelete[i];
                if (props.ContainsKey(key))
                {
                    props.Remove(key);
                }
            }
            player.Properties = new Dictionary<string, object>();
            player.SetProperties(props);
            emitPlayerProp(player.Properties);
        }
        public static void UserProp(string userId, Action<Dictionary<string, object>, Dictionary<string, object>> callback)
        {
            n.Emit(SceneEvent.UserProp, new object[] { userId }, (object[] obj) => {
                callback(((Dictionary<object, object>)obj[0]).ConverString(), ((Dictionary<object, object>)obj[1]).ConverString());
            });
        }

        internal static void emitPlayerProp(Dictionary<string, object> prop)
        {
            n.Emit(SceneEvent.ChangeProp, new object[] { prop });
        }

        public void OnError(string message)
        {
            throw new NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            switch (eb)
            {
                case SceneEvent.TeamInvite:
                    if (OnInvitedTeam != null)
                        OnInvitedTeam((string)param[0], (string)param[1]);
                    break;
                case SceneEvent.GameConnect:
                    if (OnJoinedGame != null)
                        OnJoinedGame((string)param[0]);
                    break;
                case SceneEvent.Prop:
                    var userId = (string)param[0];
                    player.UserId = userId;
                    player.AttachInfo(((Dictionary<object, object>)param[1]).ConverString());
                    player.InternalProperties(((Dictionary<object, object>)param[2]).ConverString());
                    if (OnPlayerInit != null) OnPlayerInit(player);
                    break;

            }
            return null;
        }
    }
}

