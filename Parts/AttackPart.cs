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

        private SkillInfo cSkillInfo;
        private SkillEntity cSkillEntity;
        private bool casting;
        private bool waitCast;
        private Action<SkillInfo> beforeCast;
        private Action<SkillInfo> afterCast;
        private Action<SkillInfo> readyCast;
        private Action<SkillInfo> startCast;
        private Action<SkillInfo> endCast;

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

        public void SetSkillNotic(Action<SkillInfo> before, Action<SkillInfo> after, Action<SkillInfo> start, Action<SkillInfo> end, Action<SkillInfo> ready) {
            beforeCast = before;
            afterCast = after;
            startCast = start;
            endCast = end;
            readyCast = ready;
        }

        public void Cast()
        {
            if (waitCast)
            {
                waitCast = false;
                cSkillEntity.ReadyEnd();
            }
        }

        public void CancelCast()
        {
            if (waitCast || casting)
            {
                waitCast = false;
                casting = false;
                cSkillEntity.CancelCast();
            }
        }

        private void BeforeCast()
        {
            beforeCast(cSkillInfo);
            notic(cSkillInfo);
        }

        private void AfterCast()
        {
            afterCast(cSkillInfo);
            cSkillInfo = null;
            cSkillEntity = null;
        }

        private void StartCast()
        {
            casting = true;
            startCast(cSkillInfo);
            notic(cSkillInfo);
        }

        private void EndCast()
        {
            casting = false;
            endCast(cSkillInfo);
            cSkillInfo = null;
            cSkillEntity = null;
        }

        private void ReadyCast()
        {
            readyCast(cSkillInfo);
            waitCast = true;
        }

        public void Cast(SkillInfo skillInfo, SkillEntity skillEntity, GameObject target)
        {
            skillEntity.Set(target);
            Cast(skillInfo, skillEntity);
        }

        public void Cast(SkillInfo skillInfo, SkillEntity skillEntity, Transform transform)
        {
            skillEntity.Set(transform);
            Cast(skillInfo, skillEntity);
        }

        public void Cast(SkillInfo skillInfo, SkillEntity skillEntity)
        {
            cSkillInfo = skillInfo;
            cSkillEntity = skillEntity;
            cSkillEntity.Init(BeforeCast, AfterCast, StartCast, EndCast, ReadyCast, GetAttribute("auto") > 0);
        }
    }
}
