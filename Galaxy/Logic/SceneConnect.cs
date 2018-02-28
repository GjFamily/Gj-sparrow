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
    public interface SceneDelegate{
        void OnJoinedGame(string token);
        void OnInvitedTeam(string userId, string teamId);
        void OnPlayInit(NetworkPlayer player);
    }
    public class LobbyType{
        public const string PVE = "pve";
        public const string PVP = "pvp";
    }
    // player 信息
    // lobby 信息
    // team和game接入信息
    public class SceneConnect : NamespaceListener
    {
        private static Namespace n;
        public static SceneDelegate Delegate;
        private static SceneConnect lisenter;

        private Action<bool> OnConnectAction;

        public static NetworkPlayer player;

        static SceneConnect(){
            n = PeerClient.Of(NamespaceId.Scene);
            lisenter = new SceneConnect();
            n.listener = lisenter;
        }

        public static Namespace Of(SceneRoom ns){
            return n.Of((byte)ns);
        }

        public static void Connect(Action<bool> a){
            lisenter.OnConnectAction = a;
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

        public void OnConnect(bool success)
        {
            if (OnConnectAction != null){
                OnConnectAction(success);
            }
        }

        public void OnReconnect(bool success)
        {
            throw new NotImplementedException();
        }

        public void OnError(string message)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnect()
        {
            throw new NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            switch (eb)
            {
                case SceneEvent.TeamInvite:
                    Delegate.OnInvitedTeam((string)param[0], (string)param[1]);
                    break;
                case SceneEvent.GameConnect:
                    Delegate.OnJoinedGame((string)param[1]);
                    break;
                case SceneEvent.Prop:
                    player = new NetworkPlayer(true, -1, (string)param[0], (string)param[1]);
                    player.InternalProperties(new Hashtable((Dictionary<object,object>)param[2]));
                    Delegate.OnPlayInit(player);
                    break;
                    
            }
            return null;
        }
    }
}

