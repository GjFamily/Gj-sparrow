using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic{
    public class NetworkPlayer : IComparable<NetworkPlayer>, IComparable<int>, IEquatable<NetworkPlayer>, IEquatable<int>
    {
        public int Id
        {
            get { return this.actorId; }
        }

        internal int actorId = -1;

        public string UserId { get; internal set; }

        public readonly bool IsLocal = false;

        /// <summary>Players might be inactive in a room when PlayerTTL for a room is > 0. If true, the player is not getting events from this room (now) but can return later.</summary>
        public bool IsInactive { get; set; }    // needed for rejoins
        public bool IsReady { get; set; }    // needed for rejoins

        public Hashtable CustomProperties { get; internal set; }

        public object TagObject;

        public int number;
        public int initNumber;
        public int InitProcess
        {
            get
            {
                if (initNumber == 0) return 0;
                if (number >= initNumber) return 100;
                return (initNumber - number) * 100 / initNumber;
            }
        }
        private Dictionary<string, object> info;
        public Dictionary<string, object> Info
        {
            get
            {
                return info;
            }
        }

        public NetworkPlayer(bool isLocal, int actorId, string userId)
        {
            this.CustomProperties = new Hashtable();
            this.IsLocal = isLocal;
            this.actorId = actorId;
            this.UserId = userId;
        }

        internal void AttachInfo(Dictionary<string, object> info)
        {
            this.info = info;
        }

        public override bool Equals(object p)
        {
            NetworkPlayer pp = p as NetworkPlayer;
            return (pp != null && this.GetHashCode() == pp.GetHashCode());
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        internal void InternalChangeLocalId(int newId)
        {
            if (!this.IsLocal)
            {
                Debug.LogError("ERROR You should never change PhotonPlayer IDs!");
                return;
            }

            this.actorId = newId;
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

        internal void SetCustomProperties(Hashtable propertiesToSet)
        {
            if (propertiesToSet == null)
            {
                return;
            }

            this.InternalProperties(propertiesToSet);
        }

        #region IComparable implementation

        public int CompareTo(NetworkPlayer other)
        {
            if (other == null)
            {
                return 0;
            }

            return this.GetHashCode().CompareTo(other.GetHashCode());
        }

        public int CompareTo(int other)
        {
            return this.GetHashCode().CompareTo(other);
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(NetworkPlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public bool Equals(int other)
        {
            return this.GetHashCode().Equals(other);
        }

        #endregion

        /// <summary>
        /// Brief summary string of the PhotonPlayer. Includes name or player.ID and if it's the Master Client.
        /// </summary>
        public override string ToString()
        {
            return string.Format("'{0}'{1}{2}", this.actorId, this.IsInactive ? " (inactive)" : " ", GameConnect.isMasterClient ? "(master)" : "");
        }

        /// <summary>
        /// String summary of the PhotonPlayer: player.ID, name and all custom properties of this user.
        /// </summary>
        /// <remarks>
        /// Use with care and not every frame!
        /// Converts the customProperties to a String on every single call.
        /// </remarks>
        public string ToStringFull()
        {
            return string.Format("#{0:00} '{1}'{2} {3}", this.Id, this.actorId, this.IsInactive ? " (inactive)" : "", this.CustomProperties.ToStringFull());
        }

    }
}

