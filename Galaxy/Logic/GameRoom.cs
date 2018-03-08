using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Gj.Galaxy.Network;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic{

    public class GameRoom
    {
        private GameRoomListener Delegate;
        public int MasterClientId = 0;
        public int LocalClientId = 0;
        public Dictionary<int, NetworkPlayer> mActors = new Dictionary<int, NetworkPlayer>();

        public NetworkPlayer[] mOtherPlayerListCopy = new NetworkPlayer[0];
        public NetworkPlayer[] mPlayerListCopy = new NetworkPlayer[0];

        public Hashtable CustomProperties { get; internal set; }
        public int number;
        public int initNumber;

        public int InitProcess
        {
            get{
                if (initNumber == 0) return 0;
                if (number >= initNumber) return 100;
                return (initNumber - number) * 100 / initNumber;
            }
        }

        public NetworkPlayer localPlayer
        {
            get
            {
                return GetPlayerWithId(LocalClientId);
            }
        }

        public NetworkPlayer masterClient
        {
            get
            {
                return mActors[MasterClientId];
            }
        }

        public bool isMasterClient
        {
            get
            {
                return LocalClientId == MasterClientId;
            }
        }

        public GameRoom(GameRoomListener Delegate)
        {
            this.Delegate = Delegate;
            var player = new NetworkPlayer(true, -1, SceneConnect.player.UserId);
            AddNewPlayer(player);
        }

        internal void OnFail(string reason){
            this.Delegate.OnFail(reason);
        }

        public void SetCustomProperties(Hashtable propertiesToSet)
        {
            if (propertiesToSet == null)
            {
                return;
            }

            Hashtable customProps = propertiesToSet;

            if (!PeerClient.offlineMode)
            {
                GameConnect.EmitRoom(propertiesToSet);
            }
            else
            {
                this.InternalProperties(customProps);
            }
        }

        internal void InternalProperties(Hashtable properties)
        {
            if (properties == null || properties.Count == 0 || this.CustomProperties.Equals(properties))
            {
                return;
            }

            this.CustomProperties.MergeStringKeys(properties);
            this.CustomProperties.StripKeysWithNullValues();
        }

        public void ChangeLocalId(int newID)
        {
            if (SceneConnect.player == null)
            {
                Debug.LogWarning(string.Format("LocalPlayer is null or not in mActors! LocalPlayer: {0} mActors==null: {1} newID: {2}", this.localPlayer, this.mActors == null, newID));
            }
            NetworkPlayer player;
            var result = this.mActors.TryGetValue(LocalClientId, out player);
            if(result)
            {
                this.mActors.Remove(LocalClientId);
            }
            player.actorId = newID;

            LocalClientId = newID;
            this.mActors[LocalClientId] = player;
            this.RebuildPlayerListCopies();
        }

        internal void CheckMasterClient(int leavingPlayerId)
        {
            bool currentMasterIsLeaving = this.MasterClientId == leavingPlayerId;
            bool someoneIsLeaving = leavingPlayerId > 0;

            // return early if SOME player (leavingId > 0) is leaving AND it's NOT the current master
            if (someoneIsLeaving && !currentMasterIsLeaving)
            {
                return;
            }

            this.MasterClientId = ReturnLowestPlayerId(this.mActors.Values.GetEnumerator(), leavingPlayerId);
        }

        private static int ReturnLowestPlayerId(IEnumerator<NetworkPlayer> players, int playerIdToIgnore)
        {
            if (players == null || !players.MoveNext())
            {
                return -1;
            }

            int lowestActorNumber = Int32.MaxValue;
            do
            {
                NetworkPlayer player = players.Current;
                if (player.Id == playerIdToIgnore)
                {
                    continue;
                }

                if (player.Id < lowestActorNumber)
                {
                    lowestActorNumber = player.Id;
                }
            } while (players.MoveNext());

            return lowestActorNumber;
        }

        protected internal NetworkPlayer GetPlayerWithId(int number)
        {
            if (this.mActors == null) return null;

            NetworkPlayer player = null;
            this.mActors.TryGetValue(number, out player);
            return player;
        }

        private Hashtable GetLocalActorProperties()
        {
            NetworkPlayer player = localPlayer;
            if (player != null)
            {
                return player.CustomProperties;
            }
            return null;
        }

        internal void SwitchMaster(int actor){
            NetworkPlayer player = GetPlayerWithId(actor);
            if(player != null)
                this.MasterClientId = actor;
        }

        internal void OnEnter(int localActor)
        {
            ChangeLocalId(localActor);
            Delegate.OnEnter();
        }

        internal void OnLeave(int actorId)
        {
            // actorNr is fetched out of event
            NetworkPlayer player = GetPlayerWithId(actorId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Leave for unknown player ID: {0}", actorId));
                return;
            }

            player.IsInactive = false;
            // Delegate
            Delegate.OnPlayerLeave(player);

            //this.CheckMasterClient(actorId);
        }

        internal void OnJoin(int actorId, string userId, Hashtable props)
        {
            NetworkPlayer target;
            bool exist = false;

            //var newName = (string)props[PlayerProperties.Name];
            //var userId = (string)props[PlayerProperties.UserId];
            //if(userId == localPlayer.UserId){
                
            //}
            target = GetPlayerWithId(actorId);
            if (target == null)
            {
                target = new NetworkPlayer(false, actorId, userId);
                AddNewPlayer(target);
            }
            else
            {
                exist = true;
            }

            target.InternalProperties(props);

            target.IsInactive = true;// Delegate
            if (exist)
            {
                Delegate.OnPlayerRejoin(target);
            }
            else
            {
                AuthConnect.User(userId, (Dictionary<string, object> obj) => {
                    target.AttachInfo(obj);
                    Delegate.OnPlayerJoin(target);
                });
            }
        }
        internal void OnChangeRoom(int sendId, Hashtable roomProperties)
        {
            InternalProperties(roomProperties);
            Delegate.OnRoomChange(roomProperties);
        }

        internal void OnChangePlayer(int actorId, Hashtable playerProperties)
        {
            NetworkPlayer player = GetPlayerWithId(actorId);
            player.InternalProperties(playerProperties);
        }

        internal void OnReady(int actorId)
        {
            NetworkPlayer player = GetPlayerWithId(actorId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Leave for unknown player ID: {0}", actorId));
                return;
            }
            player.IsReady = true;
            Delegate.OnReadyPlayer(player);
        }

        internal void OnReadyAll()
        {
            var enumerator = this.mActors.Values.GetEnumerator();
            while(enumerator.MoveNext()){
                NetworkPlayer player = enumerator.Current;
                player.IsReady = true;
            }
            this.CheckMasterClient(0);
            // Delegate
            Delegate.OnReadyAll();
        }

        internal void OnInit(int actorId, int number)
        {
            if(actorId == 0){
                this.initNumber = number;
                return;
            }
            NetworkPlayer player = GetPlayerWithId(actorId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Leave for unknown player ID: {0}", actorId));
                return;
            }
            player.initNumber = number;

        }

        internal void OnInstance(int actorId)
        {
            if(actorId == 0){
                this.number++;
            }
            NetworkPlayer player = GetPlayerWithId(actorId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Leave for unknown player ID: {0}", actorId));
                return;
            }
            player.number++;
        }

        internal void AddNewPlayer(NetworkPlayer player)
        {
            if (!this.mActors.ContainsKey(player.Id))
            {
                this.mActors[player.Id] = player;
                RebuildPlayerListCopies();
            }
            else
            {
                Debug.LogError("Adding player twice: " + player.Id);
            }
        }

        internal void RemovePlayer(int Id, NetworkPlayer player)
        {
            this.mActors.Remove(Id);
            if (!player.IsLocal)
            {
                RebuildPlayerListCopies();
            }
        }

        private void RebuildPlayerListCopies()
        {
            this.mPlayerListCopy = new NetworkPlayer[this.mActors.Count];
            this.mActors.Values.CopyTo(this.mPlayerListCopy, 0);

            List<NetworkPlayer> otherP = new List<NetworkPlayer>();
            for (int i = 0; i < this.mPlayerListCopy.Length; i++)
            {
                NetworkPlayer player = this.mPlayerListCopy[i];
                if (!player.IsLocal)
                {
                    otherP.Add(player);
                }
            }

            this.mOtherPlayerListCopy = otherP.ToArray();
        }

        public NetworkPlayer Find(int ID)
        {
            return GetPlayerWithId(ID);
        }

        public NetworkPlayer Get(int id)
        {
            return Find(id);
        }

        public NetworkPlayer GetNext()
        {
            return GetNextFor(SceneConnect.player);
        }

        public NetworkPlayer GetNextFor(NetworkPlayer currentPlayer)
        {
            if (currentPlayer == null)
            {
                return null;
            }
            return GetNextFor(currentPlayer.Id);
        }

        public NetworkPlayer GetNextFor(int currentPlayerId)
        {
            if (mActors.Count < 2)
            {
                return null;
            }

            Dictionary<int, NetworkPlayer> players = mActors;
            int nextHigherId = int.MaxValue;    // we look for the next higher ID
            int lowestId = currentPlayerId;     // if we are the player with the highest ID, there is no higher and we return to the lowest player's id

            foreach (int playerid in players.Keys)
            {
                if (playerid < lowestId)
                {
                    lowestId = playerid;        // less than any other ID (which must be at least less than this player's id).
                }
                else if (playerid > currentPlayerId && playerid < nextHigherId)
                {
                    nextHigherId = playerid;    // more than our ID and less than those found so far.
                }
            }

            return (nextHigherId != int.MaxValue) ? players[nextHigherId] : players[lowestId];
        }

        internal void UpdatedActorList(int[] actorsInRoom)
        {
            for (int i = 0; i < actorsInRoom.Length; i++)
            {
                int actorNrToCheck = actorsInRoom[i];
                if (SceneConnect.player.Id != actorNrToCheck && !this.mActors.ContainsKey(actorNrToCheck))
                {
                    this.AddNewPlayer(new NetworkPlayer(false, actorNrToCheck, string.Empty));
                }
            }
        }

        internal void Clear()
        {
            mActors.Clear();
            localPlayer.initNumber = 0;
            localPlayer.number = 0;
        }
    }
}
