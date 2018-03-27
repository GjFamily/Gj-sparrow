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
            Enemy,
            Empty
        }

        public void SetIdentity(Identity i)
        {
            identity = i;
        }

        public bool IsPartner()
        {
            return identity == Identity.Partner;
        }

        public bool IsEnemy()
        {
            return identity == Identity.Enemy;
        }

        public bool IsPartner(GameObject obj)
        {
            RelationPart relation = CoreTools.GetMaster(obj).GetComponent<RelationPart>();
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
            if (identity == Identity.Enemy)
            {
                return relation.identity == Identity.Enemy;
            }
            else if (identity == Identity.Partner)
            {
                return relation.identity == Identity.Partner;
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(GameObject obj)
        {
            RelationPart relation = CoreTools.GetMaster(obj).GetComponent<RelationPart>();
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
            if (identity == Identity.Enemy)
            {
                return relation.identity == Identity.Partner;
            }
            else if (identity == Identity.Partner)
            {
                return relation.identity == Identity.Enemy;
            }
            else
            {
                return false;
            }
        }


    }
}
