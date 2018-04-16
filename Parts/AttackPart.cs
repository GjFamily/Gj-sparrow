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
        private Action<SkillInfo> consume;
        private Func<SkillInfo, bool> inspect;
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
            attackDistance = GetAttribute("radio") + CoreTools.GetAttribute(obj, "radio");
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void SetPower(Func<SkillInfo, bool> func, Action<SkillInfo> action)
        {
            inspect = func;
            consume = action;
        }

        public void SetSkillNotic(Action<SkillInfo> before, Action<SkillInfo> after, Action<SkillInfo> start, Action<SkillInfo> end, Action<SkillInfo> ready) {
            beforeCast = before;
            afterCast = after;
            startCast = start;
            endCast = end;
            readyCast = ready;
        }

        public void OkCast()
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
                cSkillEntity.Cancel();
            }
        }

        private void BeforeCast()
        {
            if (beforeCast != null) beforeCast(cSkillInfo);
            if (consume != null) consume(cSkillInfo);
        }

        private void AfterCast()
        {
            if (afterCast != null) afterCast(cSkillInfo);
            cSkillInfo = null;
            cSkillEntity = null;
        }

        private void StartCast()
        {
            casting = true;
            if (startCast != null) startCast(cSkillInfo);
            if (consume != null) consume(cSkillInfo);
        }

        private void EndCast()
        {
            casting = false;
            if (endCast != null) endCast(cSkillInfo);
            cSkillInfo = null;
            cSkillEntity = null;
        }

        private void ReadyCast()
        {
            if (readyCast != null) readyCast(cSkillInfo);
            waitCast = true;
        }

        public void Cast(string skillName) {
            SkillInfo skillInfo = SkillService.single.GetSkillInfo(skillName);
            if (skillInfo != null && inspect != null && inspect(skillInfo)) {
                SkillEntity skillEntity = SkillService.single.InitSkill(skillName, gameObject);
                Cast(skillInfo, skillEntity);
            }
        }

        public void Cast(string skillName, GameObject target)
        {
            SkillInfo skillInfo = SkillService.single.GetSkillInfo(skillName);
            if (skillInfo != null && skillInfo.AllowTarget(target) && skillInfo.AllowRange(gameObject, target) && inspect != null && inspect(skillInfo)) {
                SkillEntity skillEntity = SkillService.single.InitSkill(skillName, gameObject);
                skillEntity.Set(target);
                Cast(skillInfo, skillEntity);
            }
        }

        public void Cast(string skillName, Transform transform)
        {
            SkillInfo skillInfo = SkillService.single.GetSkillInfo(skillName);
            if (skillInfo != null && skillInfo.AllowRange(gameObject, transform) && inspect != null && inspect(skillInfo)) {
                SkillEntity skillEntity = SkillService.single.InitSkill(skillName, gameObject);
                skillEntity.Set(transform);
                Cast(skillInfo, skillEntity);
            }
        }

        public void Cast(SkillInfo skillInfo, SkillEntity skillEntity)
        {
            cSkillInfo = skillInfo;
            cSkillEntity = skillEntity;
            cSkillEntity.Init(BeforeCast, AfterCast, StartCast, EndCast, ReadyCast, GetAttribute("auto") > 0);
        }
    }
}
