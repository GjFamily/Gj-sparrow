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

        protected SkillInfoPart GetSkillInfo(string skillName) {
            return GetComponent<AttackPart>().GetSkillInfo(skillName);
        }

        protected virtual void Attack(SkillInfoPart skillInfo) {
            if(IsEnoughConsume(skillInfo)){
                GetComponent<AttackPart>().Cast(skillInfo);
            }
        }

        protected virtual void Attack(SkillInfoPart skillInfo, GameObject target)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, target);
            }
        }

        protected virtual void Attack(SkillInfoPart skillInfo, Transform transform)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, transform);
            }
        }

        protected virtual void Die () {}

        protected virtual void Damaged(SkillInfoPart skillInfo, GameObject obj) { }

        protected virtual bool IsEnoughConsume(SkillInfoPart skillInfo) { return true; }

        protected virtual void Consume(SkillInfoPart skillInfo) { }
    }
}
