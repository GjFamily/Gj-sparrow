using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(AttackPart))]
    [RequirePart(typeof(DefensePart))]
    public class TargetEntity : BaseEntity
    {
        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(Damaged);
            GetComponent<AttackPart>().SetNotic(Consume);
        }

        public void SetSkillSystem(SkillSystem system)
        {
            GetComponent<AttackPart>().SetSkillSystem(system);
        }

        protected SkillInfo GetSkillInfo(string skillName) {
            return GetComponent<AttackPart>().GetSkillInfo(skillName);
        }

        protected virtual void Attack(SkillInfo skillInfo) {
            if(IsEnoughConsume(skillInfo)){
                GetComponent<AttackPart>().Cast(skillInfo);
            }
        }

        protected virtual void Attack(SkillInfo skillInfo, GameObject target)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, target);
            }
        }

        protected virtual void Attack(SkillInfo skillInfo, Transform transform)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, transform);
            }
        }

        protected virtual void Die () {}

        protected virtual void Damaged(SkillInfo skillInfo, GameObject obj) { }

        protected virtual bool IsEnoughConsume(SkillInfo skillInfo) { return true; }

        protected virtual void Consume(SkillInfo skillInfo) { }
    }
}
