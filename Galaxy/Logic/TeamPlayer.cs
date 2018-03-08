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

        internal bool update(Dictionary<string, object> teamInfo){
            bool flag = false;
            var status = (bool)teamInfo["status"];
            if (Status != status) {
                flag = true;
                Status = status;
            }
            var location = (string)teamInfo["location"];
            if (Location != location){
                flag = true;
                Location = location;
            }
            var master = (bool)teamInfo["master"];
            if (Master != master){
                flag = true;
                Master = master;
            }
            return flag;
        }

        internal void AttachInfo(Dictionary<string, object> info){
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

