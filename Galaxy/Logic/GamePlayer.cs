using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic{
    public class GamePlayer : IEquatable<GamePlayer>, IEquatable<string>
    {
        public string UserId { get; internal set; }

        public readonly bool IsLocal = false;

        /// <summary>Players might be inactive in a room when PlayerTTL for a room is > 0. If true, the player is not getting events from this room (now) but can return later.</summary>
        public bool IsInactive { get; set; }    // needed for rejoins
        public bool IsReady { get; set; }    // needed for rejoins

        public Dictionary<string, object> Properties { get; internal set; }

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

        public GamePlayer(bool isLocal, string userId)
        {
            this.Properties = new Dictionary<string, object>();
            this.IsLocal = isLocal;
            this.UserId = userId;
        }

        internal void AttachInfo(Dictionary<string, object> _info)
        {
            this.info = _info;
        }

        internal void InternalProperties(Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0 || this.Properties.Equals(properties))
            {
                return;
            }

            this.Properties.MergeStringKeys(properties);
            this.Properties.StripKeysWithNullValues();
        }

        internal void SetProperties(Dictionary<string, object> propertiesToSet)
        {
            if (propertiesToSet == null)
            {
                return;
            }

            this.InternalProperties(propertiesToSet);
        }

        #region IEquatable implementation

        public bool Equals(GamePlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return UserId.Equals(other.UserId);
        }

        public bool Equals(string other)
        {
            return UserId.Equals(other);
        }

        #endregion

        /// <summary>
        /// Brief summary string of the PhotonPlayer. Includes name or player.ID and if it's the Master Client.
        /// </summary>
        public override string ToString()
        {
            return string.Format("'{0}'{1}", this.UserId, this.IsInactive ? " (inactive)" : " ");
        }

        /// <summary>
        /// String summary of the NetworkPlayer: player.ID, name and all custom properties of this user.
        /// </summary>
        /// <remarks>
        /// Use with care and not every frame!
        /// Converts the customProperties to a String on every single call.
        /// </remarks>
        public string ToStringFull()
        {
            return string.Format("#{0} '{1}'{2}", this.UserId, this.IsInactive ? " (inactive)" : "", this.Properties.ToStringFull());
        }

    }
}

