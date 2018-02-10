using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RelationPart : BasePart
    {
        private Identity identity = Identity.Empty;
        public enum Identity
        {
            Partner,
            Monster,
            Player,
            Empty
        }

        public void SetIdentity(Identity i)
        {
            identity = i;
        }

        public Identity GetIdentity()
        {
            return identity;
        }

        public bool IsPartner(GameObject obj)
        {
            RelationPart relation = obj.GetComponent<RelationPart>();
            if (relation != null)
            {
                return IsPartner(relation);
            }
            else
            {
                return false;
            }
        }

        public bool IsPartner(RelationPart relation)
        {
            if (GetIdentity() == Identity.Monster)
            {
                return relation.GetIdentity() == Identity.Monster;
            }
            else if (GetIdentity() == Identity.Partner || GetIdentity() == Identity.Player)
            {
                return relation.GetIdentity() == Identity.Partner || relation.GetIdentity() == Identity.Player;
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(GameObject obj)
        {
            RelationPart relation = obj.GetComponent<RelationPart>();
            if (relation != null)
            {
                return IsEnemy(relation);
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(RelationPart relation)
        {
            if (GetIdentity() == Identity.Monster)
            {
                return relation.GetIdentity() == Identity.Partner || relation.GetIdentity() == Identity.Player;
            }
            else if (GetIdentity() == Identity.Partner || GetIdentity() == Identity.Player)
            {
                return relation.GetIdentity() == Identity.Monster;
            }
            else
            {
                return false;
            }
        }


    }
}
