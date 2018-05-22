using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;
using System;

namespace Gj.Galaxy.Logic{
    internal class TeamEvent
    {
        public const byte Members = 0;
        public const byte Change = 1;
        public const byte Invite = 2;
        public const byte Join = 3;
        public const byte Leave = 4;
        public const byte Lobby = 5;
    }

    public class TeamConnect : NetworkListener, NamespaceListener
    {
        public delegate void OnChangeDelegate(TeamPlayer player);
        public delegate void OnJoinPlayerDelegate(TeamPlayer player);
        public delegate void OnLeavePlayerDelegate(TeamPlayer player);
        public delegate void OnUpdateDelegate();
        public delegate void OnJoinLobbyDelegate();

        public event OnChangeDelegate OnChange;
        public event OnJoinPlayerDelegate OnJoinPlayer;
        public event OnLeavePlayerDelegate OnLeavePlayer;
        public event OnUpdateDelegate OnUpdate;
        public event OnJoinLobbyDelegate OnJoinLobby;

        private static Namespace n;
        private static TeamConnect listener;
        public static TeamConnect Listener
        {
            get
            {
                return listener;
            }
        }

        public Dictionary<string, TeamPlayer> TeamPlayers = new Dictionary<string, TeamPlayer>();

        static TeamConnect(){
			n = PeerClient.Of(NamespaceId.Team);
            listener = new TeamConnect();
            n.listener = listener;
            listener.OnConnectEvent += (success) => {
                TeamPlayer player = new TeamPlayer(SceneConnect.player.UserId);
                if (listener.OnJoinPlayer != null) listener.OnJoinPlayer(player);
            };
        }

        public static void Join(string teamName){
            n.Connect("team="+teamName);
        }

        public static void Members(){
            n.Emit(TeamEvent.Members, null);
        }

        public static void Change(bool status, string location){
            n.Emit(TeamEvent.Change, new object[] { status, location });
        }

        public static void Lobby(string lobby, Dictionary<string, object> options){
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

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            string _userId;
            TeamPlayer _player;
            switch (eb)
            {
                //case TeamEvent.Status:
                    //var members = (Dictionary<string, object>)param[0];
                    //// 先更新离开用户
                    //var leaveEnumerator = Members.GetEnumerator();
                    //while(leaveEnumerator.MoveNext())
                    //{
                    //    var current = leaveEnumerator.Current;
                    //    var userId = current.Key;
                    //    if(!members.ContainsKey(userId))
                    //    {
                    //        Members.Remove(userId);
                    //        if (OnLeavePlayer != null) OnLeavePlayer(current.Value);
                    //    }
                    //}

                    //var joinEnumerator = members.GetEnumerator();
                    //while(joinEnumerator.MoveNext())
                    //{
                    //    var current = joinEnumerator.Current;
                    //    var userId = current.Key;
                    //    TeamPlayer player;
                    //    var result = Members.TryGetValue(userId, out player);
                    //    if(result)
                    //    {
                    //        // 用户更新
                    //        var r = player.update((Dictionary<string, object>)current.Value);
                    //        if (r && OnChange != null) OnChange(player);
                    //    }
                    //    else
                    //    {
                    //        // 用户加入
                    //        player = new TeamPlayer(userId);
                    //        AuthConnect.User(userId, (Dictionary<string, object> obj) => {
                    //            player.attachInfo(obj);
                    //            if (OnJoinPlayer != null) OnJoinPlayer(player);
                    //        });
                    //        Members.Add(userId, player);
                    //        player.update((Dictionary<string, object>)current.Value);
                    //    }
                    //}
                    //if (OnUpdate != null) OnUpdate();
                    //break;
                case TeamEvent.Join:
                    _userId = (string)param[0];
                    _player = new TeamPlayer(_userId);
                    AuthConnect.User(_userId, (Dictionary<string, object> obj) => {
                        _player.AttachInfo(obj);
                        if (OnJoinPlayer != null) OnJoinPlayer(_player);
                        if (OnUpdate != null) OnUpdate();
                    });
                    TeamPlayers.Add(_userId, _player);
                    _player.update((Dictionary<string, object>)param[1]);
                    break;
                case TeamEvent.Leave:
                    _userId = (string)param[0];
                    if (TeamPlayers.TryGetValue(_userId, out _player))
                    {
                        TeamPlayers.Remove(_userId);
                        if (OnLeavePlayer != null) OnLeavePlayer(_player);
                        if (OnUpdate != null) OnUpdate();
                    }
                    break;
                case TeamEvent.Change:
                    _userId = (string)param[0];
                    if (TeamPlayers.TryGetValue(_userId, out _player))
                    {
                        var r = _player.update((Dictionary<string, object>)param[1]);
                        if (r && OnChange != null) OnChange(_player);
                        if (OnUpdate != null) OnUpdate();
                    }
                   
                    break;
                case TeamEvent.Lobby:
                    if (OnJoinLobby != null) OnJoinLobby();
                    break;

            }
            return null;
        }

    }


	public class TeamPlayer : IComparable<TeamPlayer>, IComparable<string>, IEquatable<TeamPlayer>, IEquatable<string>
    {
        public string UserId { get; internal set; }

        public bool Status { get; set; }
        public string Location { get; set; }
        public bool Master { get; set; }
        private Dictionary<string, object> info;
        public Dictionary<string, object> Info
        {
            get
            {
                return info;
            }
        }

        public TeamPlayer(string userId)
        {
            this.UserId = userId;

        }
        public override int GetHashCode()
        {
            return 0;
        }

        internal bool update(Dictionary<string, object> teamInfo)
        {
            bool flag = false;
            var status = (bool)teamInfo["status"];
            if (Status != status)
            {
                flag = true;
                Status = status;
            }
            var location = (string)teamInfo["location"];
            if (Location != location)
            {
                flag = true;
                Location = location;
            }
            var master = (bool)teamInfo["master"];
            if (Master != master)
            {
                flag = true;
                Master = master;
            }
            return flag;
        }

        internal void AttachInfo(Dictionary<string, object> info)
        {
            this.info = info;
        }

        public override bool Equals(object p)
        {
            TeamPlayer pp = p as TeamPlayer;
            return (pp != null && this.GetHashCode() == pp.GetHashCode());
        }

        #region IComparable implementation

        public int CompareTo(TeamPlayer other)
        {
            if (other == null)
            {
                return 0;
            }

            return string.Compare(this.UserId, other.UserId, StringComparison.Ordinal);
        }

        public int CompareTo(string other)
        {
            return string.Compare(this.UserId, other, StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(TeamPlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return this.UserId.Equals(other.UserId);
        }

        public bool Equals(string other)
        {
            return this.UserId.Equals(other);
        }

        #endregion
    }
}
