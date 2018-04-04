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
        protected Role role = Role.Empty;
        public enum Role
        {
            Empty,
            Self,
            Partner,
            Enemy
        }
        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(SkillEffect);
            GetComponent<DefensePart>().SetNotic(ExtraEffect);
            GetComponent<AttackPart>().SetNotic(Consume);
            GetComponent<AttackPart>().SetSkillNotic(BeforeCast, AfterCast, StartCast, EndCast, ReadyCast);
        }

        public TargetEntity Init()
        {
            BefortInit();
            Appear();
            AfterInit();
            return this;
        }

        protected virtual void BefortInit()
        {
        }

        protected virtual void AfterInit()
        {
        }

        protected SkillInfo GetSkillInfo(string skillName)
        {
            return SkillManage.single.GetSkillInfo(skillName);
        }

        protected SkillEntity GetSkillEntity(string skillName)
        {
            return SkillManage.single.InitSkill(skillName, gameObject);
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

        public void CancelCast()
        {
            GetComponent<AttackPart>().CancelCast();
        }

        public void Cast()
        {
            GetComponent<AttackPart>().Cast();
        }

        public void Cast(string skillName)
        {
            SkillInfo skillInfo = GetSkillInfo(skillName);
            if (IsEnoughConsume(skillInfo))
            {
                SkillEntity skillEntity = GetSkillEntity(skillName);
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity);
            }
        }

        public void Cast(string skillName, GameObject target)
        {
            SkillInfo skillInfo = GetSkillInfo(skillName);
            if (IsEnoughConsume(skillInfo) && skillInfo.AllowTarget(gameObject, target) && skillInfo.IsAllowRange(gameObject, target))
            {
                SkillEntity skillEntity = GetSkillEntity(skillName);
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity, target);
            }
        }

        public void Cast(string skillName, Transform transform)
        {
            SkillInfo skillInfo = GetSkillInfo(skillName);
            if (IsEnoughConsume(skillInfo) && skillInfo.IsAllowRange(gameObject, transform))
            {
                SkillEntity skillEntity = GetSkillEntity(skillName);
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity, transform);
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
