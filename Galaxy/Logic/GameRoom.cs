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
        public Dictionary<int, GamePlayer> mActors = new Dictionary<int, GamePlayer>();

        public GamePlayer[] mOtherPlayerListCopy = new GamePlayer[0];
        public GamePlayer[] mPlayerListCopy = new GamePlayer[0];

        public Dictionary<string, object> CustomProperties { get; internal set; }
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

        public GamePlayer localPlayer
        {
            get
            {
                return GetPlayerWithId(LocalClientId);
            }
        }

        public GamePlayer masterClient
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
            var player = new GamePlayer(true, -1, SceneConnect.player.UserId);
            LocalClientId = -1;
            AddNewPlayer(player);
            this.CustomProperties = new Dictionary<string, object>();
        }

        internal void OnFail(string reason){
            this.Delegate.OnFail(reason);
        }

        public void SetCustomProperties(Dictionary<string, object> propertiesToSet)
        {
            if (propertiesToSet == null)
            {
                return;
            }

            Dictionary<string, object> customProps = propertiesToSet;

            if (!PeerClient.offlineMode)
            {
                GameConnect.EmitRoom(propertiesToSet);
            }
            else
            {
                this.InternalProperties(customProps);
            }
        }

        internal void InternalProperties(Dictionary<string, object> properties)
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
            GamePlayer player;
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

        private static int ReturnLowestPlayerId(IEnumerator<GamePlayer> players, int playerIdToIgnore)
        {
            if (players == null || !players.MoveNext())
            {
                return -1;
            }

            int lowestActorNumber = Int32.MaxValue;
            do
            {
                GamePlayer player = players.Current;
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

        protected internal GamePlayer GetPlayerWithId(int number)
        {
            if (this.mActors == null) return null;

            GamePlayer player = null;
            this.mActors.TryGetValue(number, out player);
            return player;
        }

        private Dictionary<string, object> GetLocalActorProperties()
        {
            GamePlayer player = localPlayer;
            if (player != null)
            {
                return player.CustomProperties;
            }
            return null;
        }

        internal void SwitchMaster(int actor){
            GamePlayer player = GetPlayerWithId(actor);
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
            GamePlayer player = GetPlayerWithId(actorId);
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

        internal void OnJoin(int actorId, string userId)
        {
            GamePlayer target;
            bool exist = false;

            //var newName = (string)props[PlayerProperties.Name];
            //var userId = (string)props[PlayerProperties.UserId];
            //if(userId == localPlayer.UserId){
                
            //}
            target = GetPlayerWithId(actorId);
            if (target == null)
            {
                target = new GamePlayer(false, actorId, userId);
                AddNewPlayer(target);
            }
            else
            {
                exist = true;
            }

            //target.InternalProperties(props);

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
        internal void OnChangeRoom(int sendId, Dictionary<string, object> roomProperties)
        {
            InternalProperties(roomProperties);
            Delegate.OnRoomChange(roomProperties);
        }

        internal void OnChangePlayer(int actorId, Dictionary<string, object> playerProperties)
        {
            GamePlayer player = GetPlayerWithId(actorId);
            player.InternalProperties(playerProperties);
        }

        internal void OnReady(int actorId)
        {
            GamePlayer player = GetPlayerWithId(actorId);
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
                GamePlayer player = enumerator.Current;
                player.IsReady = true;
            }
            //this.CheckMasterClient(0);
            // Delegate
            Delegate.OnReadyAll();
        }

        internal void OnInit(int actorId, int number)
        {
            if(actorId == 0){
                this.initNumber = number;
                return;
            }
            GamePlayer player = GetPlayerWithId(actorId);
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
                return;
            }
            GamePlayer player = GetPlayerWithId(actorId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Instance for unknown player ID: {0}", actorId));
                return;
            }
            player.number++;
        }

        internal void AddNewPlayer(GamePlayer player)
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

        internal void RemovePlayer(int Id, GamePlayer player)
        {
            this.mActors.Remove(Id);
            if (!player.IsLocal)
            {
                RebuildPlayerListCopies();
            }
        }

        private void RebuildPlayerListCopies()
        {
            this.mPlayerListCopy = new GamePlayer[this.mActors.Count];
            this.mActors.Values.CopyTo(this.mPlayerListCopy, 0);

            List<GamePlayer> otherP = new List<GamePlayer>();
            for (int i = 0; i < this.mPlayerListCopy.Length; i++)
            {
                GamePlayer player = this.mPlayerListCopy[i];
                if (!player.IsLocal)
                {
                    otherP.Add(player);
                }
            }

            this.mOtherPlayerListCopy = otherP.ToArray();
        }

        public GamePlayer Find(int ID)
        {
            return GetPlayerWithId(ID);
        }

        public GamePlayer Get(int id)
        {
            return Find(id);
        }

        public GamePlayer GetNext()
        {
            return GetNextFor(SceneConnect.player);
        }

        public GamePlayer GetNextFor(GamePlayer currentPlayer)
        {
            if (currentPlayer == null)
            {
                return null;
            }
            return GetNextFor(currentPlayer.Id);
        }

        public GamePlayer GetNextFor(int currentPlayerId)
        {
            if (mActors.Count < 2)
            {
                return null;
            }

            Dictionary<int, GamePlayer> players = mActors;
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
                    this.AddNewPlayer(new GamePlayer(false, actorNrToCheck, string.Empty));
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
