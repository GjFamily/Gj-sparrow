using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;

namespace Gj.Galaxy.Logic{
    internal enum TeamEvent : byte
    {
        
    }
    public interface TeamDelegate
    {
        void OnChange();
        void OnJoinPlayer(NetworkPlayer player);
        void OnLeavePlayer(NetworkPlayer player);
    }
    public class TeamConnect : NamespaceListener
    {
        private static Namespace n;
        public static TeamDelegate Delegate;
        private static TeamConnect listener;

        static TeamConnect(){
            n = SceneConnect.Of(SceneRoom.Team);
            listener = new TeamConnect();
            n.listener = listener;
        }

        public static void JoinTeam(string teamName){
            n.Connect("team="+teamName);
        }

        public void OnConnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public void OnDisconnect()
        {
            throw new System.NotImplementedException();
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public void OnEvent()
        {
            throw new System.NotImplementedException();
        }

        public void OnReconnect(bool success)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            throw new System.NotImplementedException();
        }
        //public bool OpCreateGame(EnterRoomParams enterRoomParams)
        //{
        //    bool onGameServer = this.Server == ServerConnection.GameServer;
        //    enterRoomParams.OnGameServer = onGameServer;
        //    enterRoomParams.PlayerProperties = GetLocalActorProperties();
        //    if (!onGameServer)
        //    {
        //        enterRoomParamsCache = enterRoomParams;
        //    }

        //    this.lastJoinType = JoinType.CreateRoom;
        //    return base.OpCreateRoom(enterRoomParams);
        //}

    }
}
