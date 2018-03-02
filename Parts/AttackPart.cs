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

        public void Cast(SkillInfo skillInfo)
        {
            gameSystem.Cast(skillInfo.name, gameObject);
            notic(skillInfo);
        }

        public void Cast(SkillInfo skillInfo, GameObject target)
        {
            if (skillInfo.AllowTarget(gameObject, target) && skillInfo.IsAllowRange(gameObject, target))
            {
                gameSystem.Cast(skillInfo.name, gameObject, target);
                notic(skillInfo);
            }
        }

        public void Cast(SkillInfo skillInfo, Transform transform)
        {
            if (skillInfo.IsAllowRange(gameObject, transform))
            {
                gameSystem.Cast(skillInfo.name, gameObject, transform);
                notic(skillInfo);
            }
        }
    }
}
