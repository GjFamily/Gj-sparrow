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

        protected virtual void Die () {}

        protected virtual void Damaged(SkillInfoPart skillInfo, GameObject obj) { }

        protected virtual bool IsEnoughConsume(SkillInfoPart skillInfo) { return true; }

        protected virtual void Consume(SkillInfoPart skillInfo) { }
    }
}
