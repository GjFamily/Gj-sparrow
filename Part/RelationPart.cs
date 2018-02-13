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

        public bool IsPartner(GameObject obj)
        {
            RelationPart relation = Tools.GetMaster(obj).GetComponent<RelationPart>();
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
            if (identity == Identity.Monster)
            {
                return relation.identity == Identity.Monster;
            }
            else if (identity == Identity.Partner || identity == Identity.Player)
            {
                return relation.identity == Identity.Partner || relation.identity == Identity.Player;
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(GameObject obj)
        {
            RelationPart relation = Tools.GetMaster(obj).GetComponent<RelationPart>();
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
            if (identity == Identity.Monster)
            {
                return relation.identity == Identity.Partner || relation.identity == Identity.Player;
            }
            else if (identity == Identity.Partner || identity == Identity.Player)
            {
                return relation.identity == Identity.Monster;
            }
            else
            {
                return false;
            }
        }


    }
}
