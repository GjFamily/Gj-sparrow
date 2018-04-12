using UnityEngine;
using System.Collections;

namespace Gj
{
    public class SkillInfo : MonoBehaviour
    {
        public float value;
        public float need;
        public float range;
        public float intervalTime;
        public TargetRelation targetRelation;

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
            Attack,
            Defense,
            Prop,
            Attr
        }
        public float readyTime;
        public float castTime;
        public float sustainedTime;
        public CastType castType;
        public enum CastType
        {
            Now,
            Ready,
            Sustained,
            ReadyAndSustained
        }

        [HideInInspector]
        public GameObject master;

        public bool AllowTarget(GameObject target)
        {
            return AllowTarget(master, target);
        }

        public bool AllowTarget(GameObject master, GameObject target)
        {
            if (targetRelation == TargetRelation.Self)
            {
                return master == target;
            }
            else
            {
                RelationPart relation = master.GetComponent<RelationPart>();
                if (relation == null) return false;
                if (targetRelation == TargetRelation.Partner)
                {
                    return relation.IsPartner(target);
                }
                else if (targetRelation == TargetRelation.Enemy)
                {
                    return relation.IsEnemy(target);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsAllowRange(GameObject master, GameObject target)
        {
            return IsAllowRange(master, target.transform);
        }

        public bool IsAllowRange(GameObject master, Transform transform)
        {
            return Vector3.Distance(master.transform.position, transform.position) <= range;
        }
    }
}
