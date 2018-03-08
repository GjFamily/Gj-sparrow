using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(AttackPart))]
    [RequirePart(typeof(DefensePart))]
    [RequirePart(typeof(StatusPart))]
    [RequirePart(typeof(RelationPart))]
    public class TargetEntity : BaseEntity
    {
        GameSystem gameSystem;

        [HideInInspector]
        public bool player = false;

        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(SkillEffect);
            GetComponent<DefensePart>().SetNotic(ExtraEffect);
            GetComponent<AttackPart>().SetNotic(Consume);
            GetComponent<AttackPart>().SetSkillNotic(BeforeCast, AfterCast, StartCast, EndCast, ReadyCast);
        }

        public virtual void Init()
        {
            Appear();
            InitFeature();
        }

        protected virtual void InitFeature()
        {

        }

        public void SetGameSystem(GameSystem system)
        {
            gameSystem = system;
        }

        protected SkillInfo GetSkillInfo(string skillName)
        {
            return gameSystem.GetSkillInfo(skillName);
        }

        protected virtual void GetPower(SkillInfo skillInfo)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, gameObject);
            }
        }

        protected virtual void BeforeCast(SkillInfo skillInfo)
        {
        }

        protected virtual void AfterCast(SkillInfo skillInfo)
        {
        }

        protected virtual void StartCast(SkillInfo skillInfo)
        {
        }

        protected virtual void EndCast(SkillInfo skillInfo)
        {
        }

        protected virtual void ReadyCast(SkillInfo skillInfo)
        {
        }

        protected void Attack(SkillInfo skillInfo)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo);
            }
        }

        protected void Attack(SkillInfo skillInfo, GameObject target)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, target);
            }
        }

        protected void Attack(SkillInfo skillInfo, Transform transform)
        {
            if (IsEnoughConsume(skillInfo))
            {
                GetComponent<AttackPart>().Cast(skillInfo, transform);
            }
        }

        protected virtual void Die()
        {
            Disappear();
        }

        protected virtual void SkillEffect(SkillInfo skillInfo) { }

        protected virtual void ExtraEffect(ExtraInfo extraInfo) { }

        protected virtual void Effect(float value) { }

        protected virtual bool IsEnoughConsume(SkillInfo skillInfo) { return true; }

        protected virtual void Consume(SkillInfo skillInfo) { }
    }
}
