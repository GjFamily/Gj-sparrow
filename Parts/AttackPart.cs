using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class AttackPart : BasePart
    {
        private GameObject target;
        private Action enterAttackNotic;
        private Action exitAttackNotic;
        private Action attackNotic;
        private Action<SkillInfo> notic;
        private float attackDistance;
        private bool attacking = false;

        private GameSystem gameSystem;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                if (Vector3.Distance(target.transform.position, gameObject.transform.position) < attackDistance)
                {
                    if (!attacking)
                    {
                        if (enterAttackNotic != null)
                        {
                            enterAttackNotic();
                        }
                        attacking = true;
                    }
                }
                else
                {
                    if (attacking)
                    {
                        if (exitAttackNotic != null)
                        {
                            exitAttackNotic();
                        }
                        attacking = false;
                    }
                }
            }
        }

        public SkillInfo GetSkillInfo(string skillName)
        {
            return gameSystem.GetSkillInfo(skillName);
        }

        public void SetGameSystem(GameSystem system)
        {
            gameSystem = system;
        }

        public void SetAttackTarget(GameObject obj, Action enterAction, Action exitAction)
        {
            target = obj;
            attackDistance = GetAttribute("radio") + GetAttribute(obj, "radio");
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void SetNotic(Action<SkillInfo> notic)
        {
            this.notic = notic;
        }

        public void Cast(SkillEntity skillEntity, SkillInfo skillInfo)
        {
            skillEntity.Cast();
            notic(skillInfo);
        }

        public void Cast(SkillInfo skillInfo)
        {
            SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
            Cast(skillInfo, skillEntity);
        }

        public void Cast(SkillInfo skillInfo, GameObject target)
        {
            if (skillInfo.AllowTarget(gameObject, target) && skillInfo.IsAllowRange(gameObject, target))
            {
                SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
                skillEntity.Set(target);
                Cast(skillInfo, skillEntity);
            }
        }

        public void Cast(SkillInfo skillInfo, Transform transform)
        {
            if (skillInfo.IsAllowRange(gameObject, transform))
            {
                SkillEntity skillEntity = gameSystem.InitSkill(skillInfo.name, gameObject);
                skillEntity.Set(transform);
                Cast(skillInfo, skillEntity);
            }
        }

        private void Cast(SkillInfo skillInfo, SkillEntity skillEntity){
            switch(skillInfo.castType){
                case SkillInfo.CastType.Now:
                    Cast(skillEntity, skillInfo);
                    break;
                case SkillInfo.CastType.Ready:
                    skillEntity.ReadyCast();
                    Game.single.WaitSeconds(skillInfo.readyTime, ()=>{
                        Cast(skillEntity, skillInfo);
                    });
                    break;
                case SkillInfo.CastType.Sustained:
                    Cast(skillEntity, skillInfo);
                    Game.single.WaitSeconds(skillInfo.sustainedTime, () => {
                        skillEntity.CancelCast();
                    });
                    break;
                case SkillInfo.CastType.ReadyAndSustained:
                    skillEntity.ReadyCast();
                    Game.single.WaitSeconds(skillInfo.readyTime, () => {
                        Cast(skillEntity, skillInfo);
                        Game.single.WaitSeconds(skillInfo.sustainedTime, () => {
                            skillEntity.CancelCast();
                        });
                    });
                    break;
            }
        }
    }
}
