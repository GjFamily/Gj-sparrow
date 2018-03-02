using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;

namespace Gj.Galaxy.Logic{

    internal class SceneEvent
    {
        public const byte LobbyJoin = 0;
        public const byte LobbyLeave = 1;
        public const byte LobbyExist = 2;
        public const byte TeamCreate = 3;
        public const byte PropChanged = 4;
        public const byte TeamInvite = 255;
        public const byte GameConnect = 254;
        public const byte Prop = 253;
    }
    public class LobbyType{
        public const string PVE = "pve";
        public const string PVP = "pvp";
    }
    // player 信息
    // lobby 信息
    // team和game接入信息
    public class SceneConnect:NetworkListener, NamespaceListener
    {
        private static Namespace n;
        private static SceneConnect listener;
        public static SceneConnect Listener{
            get
            {
                return listener;
            }
        }

        public delegate void OnJoinedGameDelegate(string token);
        public delegate void OnInvitedTeamDelegate(string userId, string teamId);
        public delegate void OnPlayInitDelegate(NetworkPlayer player);

        public event OnJoinedGameDelegate OnJoinedGame;
        public event OnInvitedTeamDelegate OnInvitedTeam;
        public event OnPlayInitDelegate OnPlayInit;

        private Action<bool> OnConnectAction;

        public static NetworkPlayer player;

        static SceneConnect(){
            n = PeerClient.Of(NamespaceId.Scene);
            listener = new SceneConnect();
            n.listener = listener;
            listener.OnConnectEvent += (success) =>
            {
                if (listener.OnConnectAction != null) listener.OnConnectAction(success);
            };
        }

        public static Namespace Of(SceneRoom ns){
            return n.Of((byte)ns);
        }

        public static void Connect(Action<bool> a){
            Listener.OnConnectAction = a;
            n.Connect("abc=123");
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
            n.Emit(SceneEvent.LobbyLeave, new object[]{});
        }

        public static void ExistLobby(Action<bool> callback)
        {
            n.Emit(SceneEvent.LobbyExist, new object[] { }, (obj) => callback((bool)obj[0]));
        }

        public static void SetCustomProperties(Hashtable customProperties)
        {
            if (customProperties == null)
            {
                customProperties = new Hashtable();
                foreach (object k in player.CustomProperties.Keys)
                {
                    customProperties[(string)k] = null;
                }
            }
            player.SetCustomProperties(customProperties);
        }
        public static void RemoveCustomProperties(string[] customPropertiesToDelete)
        {
            var props = player.CustomProperties;
            for (int i = 0; i < customPropertiesToDelete.Length; i++)
            {
                string key = customPropertiesToDelete[i];
                if (props.ContainsKey(key))
                {
                    props.Remove(key);
                }
            }
            player.CustomProperties = new Hashtable();
            player.SetCustomProperties(props);
        }

        internal static void emitPlayerProp(Hashtable prop){
            n.Emit(SceneEvent.PropChanged, new object[] { prop });
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
                        OnJoinedGame((string)param[1]);
                    break;
                case SceneEvent.Prop:
                    player = new NetworkPlayer(true, -1, (string)param[0]);
                    player.InternalProperties(new Hashtable((Dictionary<object, object>)param[1]));
                    if (OnPlayInit != null)
                        OnPlayInit(player);
                    break;

            }
            return null;
        }
    }
}

