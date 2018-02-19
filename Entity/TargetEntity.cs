using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(AttackPart))]
    [RequirePart(typeof(DefensePart))]
    public class TargetEntity : BaseEntity
    {
        protected SkillSystem skillSystem;
        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(Damaged);
        }

        public void SetSkillSystem(SkillSystem system)
        {
            skillSystem = system;
        }

        protected virtual void Damaged(SkillInfoPart skillInfo, GameObject obj) { }
    }
}
