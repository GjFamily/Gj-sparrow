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
                Info info = CoreTools.GetInfo(master);
                if (info == null) return false;
                if (targetRelation == TargetRelation.Partner)
                {
                    return info.IsPartner(target);
                }
                else if (targetRelation == TargetRelation.Enemy)
                {
                    return info.IsEnemy(target);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool AllowRange(Transform transform)
        {
            return AllowRange(master, transform);
        }

        public bool AllowRange(GameObject target)
        {
            return AllowRange(master, target);
        }

        public bool AllowRange(GameObject master, GameObject target)
        {
            return AllowRange(master, target.transform);
        }

        public bool AllowRange(GameObject master, Transform transform)
        {
            return Vector3.Distance(master.transform.position, transform.position) <= range;
        }
    }
}
