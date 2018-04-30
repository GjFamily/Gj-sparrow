using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Gj.Galaxy.Network;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic{

    public class GameRoom: PlayerFactory
    {
        internal static GameDelegate Delegate;
        public string LocalId = "";
        public Dictionary<string, GamePlayer> mPlayers = new Dictionary<string, GamePlayer>();

        public GamePlayer[] mOtherPlayerListCopy = new GamePlayer[0];
        public GamePlayer[] mPlayerListCopy = new GamePlayer[0];

        public Dictionary<string, object> Properties { get; internal set; }
        //public int number;
        //public int initNumber;

        //public int InitProcess
        //{
        //    get{
        //        if (initNumber == 0) return 0;
        //        if (number >= initNumber) return 100;
        //        return (initNumber - number) * 100 / initNumber;
        //    }
        //}

        public GamePlayer localPlayer
        {
            get
            {
                return GetPlayer(LocalId);
            }
        }

        //public GamePlayer masterClient
        //{
        //    get
        //    {
        //        return mPlayers[MasterClientId];
        //    }
        //}

        //public bool isMasterClient
        //{
        //    get
        //    {
        //        return LocalClientId == MasterClientId;
        //    }
        //}

        public GameRoom(GameDelegate @delegate)
        {
            Delegate = @delegate;
            var player = SceneConnect.player;
            AddNewPlayer(player);
            LocalId = player.UserId;
            this.Properties = new Dictionary<string, object>();
        }

        //public void SetProperties(Dictionary<string, object> propertiesToSet)
        //{
        //    if (propertiesToSet == null)
        //    {
        //        return;
        //    }

        //    Dictionary<string, object> customProps = propertiesToSet;

        //    if (!PeerClient.offlineMode)
        //    {
        //        RoomConnect.EmitRoom(propertiesToSet);
        //    }
        //    else
        //    {
        //        this.InternalProperties(customProps);
        //    }
        //}

        internal void InternalProperties(Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0 || this.Properties.Equals(properties))
            {
                return;
            }

            this.Properties.MergeStringKeys(properties);
            this.Properties.StripKeysWithNullValues();
        }

        public GamePlayer GetPlayer(string userId)
        {
            if (this.mPlayers == null) return null;
            GamePlayer player = null;
            this.mPlayers.TryGetValue(userId, out player);
            return player;
        }

        private Dictionary<string, object> GetLocalProperties()
        {
            GamePlayer player = localPlayer;
            if (player != null)
            {
                return player.Properties;
            }
            return null;
        }

        internal void OnLeave(string playerId)
        {
            // actorNr is fetched out of event
            GamePlayer player = GetPlayer(playerId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Leave for unknown player ID: {0}", playerId));
                return;
            }

            player.IsInactive = false;
            // Delegate
            Delegate.OnPlayerLeave(player);
        }

        internal void OnJoin(string userId)
        {
            GamePlayer target;
            bool exist = false;

            target = GetPlayer(userId);
            if (target == null)
            {
                target = new GamePlayer(false, userId);
                AddNewPlayer(target);
            }
            else
            {
                exist = true;
            }

            target.IsInactive = true;// Delegate
            if (exist)
            {
                Delegate.OnPlayerRejoin(target);
            }
            else
            {
                SceneConnect.UserProp(userId, (Dictionary<string, object> info, Dictionary<string, object> prop) =>
                {
                    target.AttachInfo(info);
                    target.InternalProperties(prop);
                    Delegate.OnPlayerJoin(target);
                });
            }
        }
        //internal void OnChangeRoom(Dictionary<string, object> roomProperties)
        //{
        //    InternalProperties(roomProperties);
        //    Delegate.OnRoomChange(roomProperties);
        //}

        //internal void OnChangePlayer(string userId, Dictionary<string, object> playerProperties)
        //{
        //    GamePlayer player = GetPlayer(userId);
        //    player.InternalProperties(playerProperties);
        //}

        internal void OnReady(string userId)
        {
            GamePlayer player = GetPlayer(userId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Ready for unknown player ID: {0}", userId));
                return;
            }
            player.IsReady = true;
            Delegate.OnReadyPlayer(player);
        }

        internal void OnReadyAll()
        {
            var enumerator = this.mPlayers.Values.GetEnumerator();
            while(enumerator.MoveNext()){
                GamePlayer player = enumerator.Current;
                player.IsReady = true;
            }
            //this.CheckMasterClient(0);
            // Delegate
            Delegate.OnReadyAll();
        }

        public void OnInit(string userId, int number)
        {
            //if(actorId == 0){
            //    this.initNumber = number;
            //    return;
            //}
            GamePlayer player = GetPlayer(userId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Init for unknown player ID: {0}", userId));
                return;
            }
            player.initNumber = number;

        }

        public void OnInstance(string userId)
        {
            //if(actorId == 0){
            //    this.number++;
            //    return;
            //}
            GamePlayer player = GetPlayer(userId);
            if (player == null)
            {
                Debug.LogError(String.Format("Received event Instance for unknown player ID: {0}", userId));
                return;
            }
            player.number++;
        }

        internal void AddNewPlayer(GamePlayer player)
        {
            if (!this.mPlayers.ContainsKey(player.UserId))
            {
                this.mPlayers[player.UserId] = player;
                RebuildPlayerListCopies();
            }
            else
            {
                Debug.LogError("Adding player twice: " + player.UserId);
            }
        }

        internal void RemovePlayer(string userId, GamePlayer player)
        {
            this.mPlayers.Remove(userId);
            if (!player.IsLocal)
            {
                RebuildPlayerListCopies();
            }
        }

        private void RebuildPlayerListCopies()
        {
            this.mPlayerListCopy = new GamePlayer[this.mPlayers.Count];
            this.mPlayers.Values.CopyTo(this.mPlayerListCopy, 0);

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

        public GamePlayer Find(string Id)
        {
            return GetPlayer(Id);
        }

        public GamePlayer Get(string Id)
        {
            return Find(Id);
        }

        internal void Clear()
        {
            mPlayers.Clear();
        }
    }
}
