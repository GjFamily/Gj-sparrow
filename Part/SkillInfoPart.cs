using UnityEngine;
using System.Collections;

namespace Gj
{
    public class SkillInfoPart : BasePart
    {
        public float power;
        public float need;
        public float range;
        public Relation relation;
        public enum Relation
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

        public bool IsEnough(float num)
        {
            return num > need;
        }

        public bool IsOutRange(GameObject master, GameObject target)
        {
            return IsOutRange(master, target.transform.position);
        }

        public bool IsOutRange(GameObject master, Vector3 position)
        {
            return Vector3.Distance(master.transform.position, position) > range;
        }

        public bool TargetRelationOk (GameObject master, GameObject target) {
            RelationPart relationPart = master.GetComponent<RelationPart>();
            if (relationPart == null) return false;
            if (relation == Relation.Partner) {
                return relationPart.IsPartner(target);
            } else if (relation == Relation.Enemy) {
                return relationPart.IsEnemy(target);
            } else {
                return false;
            }
        }
    }
}
