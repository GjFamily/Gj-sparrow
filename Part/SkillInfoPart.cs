using UnityEngine;
using System.Collections;

namespace Gj
{
    public class SkillInfoPart : BasePart
    {
        public string skillName;
        public float power;
        public float need;
        public NeedType needType;
        public enum NeedType
        {
            Number,
            Energy,
            Magic,
            Empty
        }
        public float range;
        public RargetRelation targetRelation;
        public float waitTime;
        public enum RargetRelation
        {
            Self,
            Partner,
            Enemy
        }
        public TargetNum targetNum;
        public enum TargetNum
        {
            One,
            Some
        }
        public TargetNeed targetNeed;
        public enum TargetNeed
        {
            Target,
            Region,
            None
        }
        public SkillType skillType;
        public enum SkillType
        {

        }

        public bool AllowTarget(GameObject master, GameObject target)
        {
            RelationPart relation = master.GetComponent<RelationPart>();
            if (relation == null) return false;
            if (targetRelation == RargetRelation.Partner)
            {
                return relation.IsPartner(target);
            }
            else if (targetRelation == RargetRelation.Enemy)
            {
                return relation.IsEnemy(target);
            }
            else
            {
                return false;
            }
        }

        public bool IsOutRange(GameObject master, GameObject target)
        {
            return IsOutRange(master, target.transform);
        }

        public bool IsOutRange(GameObject master, Transform transform)
        {
            return Vector3.Distance(master.transform.position, transform.position) > range;
        }
    }
}
