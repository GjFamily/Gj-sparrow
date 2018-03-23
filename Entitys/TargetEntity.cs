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

        protected SkillEntity GetSkillEntity(SkillInfo skillInfo)
        {
            return gameSystem.InitSkill(skillInfo.name, gameObject);
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

        protected void CancelCast()
        {
            GetComponent<AttackPart>().CancelCast();
        }

        protected void Cast()
        {
            GetComponent<AttackPart>().Cast();
        }

        protected virtual void GetPower(SkillInfo skillInfo)
        {
            SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
            if (IsEnoughConsume(skillInfo) && skillEntity != null)
            {
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity, gameObject);
            }
        }

        protected void Cast(SkillInfo skillInfo)
        {
            SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
            if (IsEnoughConsume(skillInfo) && skillEntity != null)
            {
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity);
            }
        }

        protected void Cast(SkillInfo skillInfo, GameObject target)
        {
            SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
            if (IsEnoughConsume(skillInfo) && skillEntity != null)
            {
                GetComponent<AttackPart>().Cast(skillInfo, skillEntity, target);
            }
        }

        protected void Cast(SkillInfo skillInfo, Transform transform)
        {
            SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
            if (IsEnoughConsume(skillInfo) && skillEntity != null)
            {
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
