using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(AttackPart))]
    [RequirePart(typeof(DefensePart))]
    [RequirePart(typeof(StatusPart))]
    public class TargetEntity : BaseEntity
    {
        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(SkillEffect);
            GetComponent<DefensePart>().SetNotic(ExtraEffect);
            GetComponent<StatusPart>().SetAddNotic(AddAttribute);
            GetComponent<StatusPart>().SetCancelNotic(CancelAttribute);
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

        protected virtual void AddAttribute(ExtraInfo extraInfo) { }

        protected virtual void CancelAttribute(ExtraInfo extraInfo) { }

        protected virtual void SkillEffect(SkillInfo skillInfo) { }

        protected virtual void ExtraEffect(ExtraInfo extraInfo) { }

        protected virtual void Effect(float value) { }

        protected virtual bool IsEnoughConsume(SkillInfo skillInfo) { return true; }

        protected virtual void Consume(SkillInfo skillInfo) { }
    }
}
