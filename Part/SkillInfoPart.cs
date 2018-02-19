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
        public Relation relation;
        public float waitTime;
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
    }
}
