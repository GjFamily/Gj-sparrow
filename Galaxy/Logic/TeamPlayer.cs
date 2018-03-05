using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Logic{
    public class TeamPlayer : IComparable<TeamPlayer>, IComparable<string>, IEquatable<TeamPlayer>, IEquatable<string>
    {
        public string UserId { get; internal set; }

        public bool Status { get; set; }
        public string Location { get; set; }

        public TeamPlayer(string userId)
        {
            this.UserId = userId;
        }

        public override bool Equals(object p)
        {
            TeamPlayer pp = p as TeamPlayer;
            return (pp != null && this.GetHashCode() == pp.GetHashCode());
        }

        public new string GetHashCode()
        {
            return this.UserId;
        }

        #region IComparable implementation

        public int CompareTo(TeamPlayer other)
        {
            if (other == null)
            {
                return 0;
            }

            return string.Compare(this.GetHashCode(), other.GetHashCode(), StringComparison.Ordinal);
        }

        public int CompareTo(string other)
        {
            return string.Compare(this.GetHashCode(), other, StringComparison.Ordinal);
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(TeamPlayer other)
        {
            if (other == null)
            {
                return false;
            }

            return this.GetHashCode().Equals(other.GetHashCode());
        }

        public bool Equals(string other)
        {
            return this.GetHashCode().Equals(other);
        }

        #endregion
    }
}

