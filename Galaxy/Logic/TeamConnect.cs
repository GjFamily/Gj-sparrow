using UnityEngine;
using System.Collections;
using Gj.Galaxy.Network;
using System.Collections.Generic;

namespace Gj.Galaxy.Logic{
    internal class TeamEvent
    {
        public const byte Change = 0;
        public const byte Invite = 1;
        public const byte Status = 2;
        public const byte Join = 3;
        public const byte Leave = 4;
    }
    public class TeamConnect : NetworkListener, NamespaceListener
    {
        public delegate void OnChangeDelegate(TeamPlayer player);
        public delegate void OnJoinPlayerDelegate(TeamPlayer player);
        public delegate void OnLeavePlayerDelegate(TeamPlayer player);

        public event OnChangeDelegate OnChange;
        public event OnJoinPlayerDelegate OnJoinPlayer;
        public event OnLeavePlayerDelegate OnLeavePlayer;

        private static Namespace n;
        private static TeamConnect listener;
        public static TeamConnect Listener
        {
            get
            {
                return listener;
            }
        }


        public Dictionary<string, TeamPlayer> members = new Dictionary<string, TeamPlayer>();

        static TeamConnect(){
            n = SceneConnect.Of(SceneRoom.Team);
            listener = new TeamConnect();
            n.listener = listener;
        }

        public static void Join(string teamName){
            n.Connect("team="+teamName);
        }

        public static void Change(bool status, string location){
            n.Emit(TeamEvent.Change, new object[] { status, location });
        }

        public void OnError(string message)
        {
            throw new System.NotImplementedException();
        }

        public object[] OnEvent(byte eb, object[] param)
        {
            switch (eb)
            {
                case TeamEvent.Status:
                    var members = (Dictionary<string, object>)param[0];
                    members.GetEnumerator();
                    break;
                case TeamEvent.Join:
                    break;
                case TeamEvent.Leave:
                    break;

            }
            return null;
        }

    }
}
