using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RelationPart : BasePart
    {
        private Identity identity;
        public enum Identity
        {
            Partner,
            Monster,
            Player,
            Empty,
            Skill
        }

        public void SetIdentity(Identity i)
        {
            identity = i;
        }

        public Identity GetIdentity()
        {
            return identity;
        }

        public bool IsTarget()
        {
            return IsTarget(this);
        }

        public bool IsTarget(RelationPart relation)
        {

            if (relation.GetIdentity() != Identity.Skill && relation.GetIdentity() != Identity.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsSkill()
        {
            return IsSkill(this);
        }

        public bool IsSkill(RelationPart relation)
        {
            if (relation.GetIdentity() == Identity.Skill)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsPartner(RelationPart relation)
        {
            return true;
        }

        public bool IsEnemy(RelationPart relation)
        {
            return true;
        }


    }
}
