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
        private Action<SkillInfoPart> notic;
        private float attackDistance;
        private bool attacking = false;

        private SkillSystem skillSystem;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                if (Vector3.Distance(target.transform.position, gameObject.transform.position) < attackDistance) {
                    if (!attacking) {
                        if (enterAttackNotic != null) {
                            enterAttackNotic();
                        }
                        attacking = true;
                    }
                } else {
                    if (attacking) {
                        if (exitAttackNotic != null) {
                            exitAttackNotic();
                        }
                        attacking = false;
                    }
                }
            }
        }

        public void SetSkillSystem (SkillSystem system) {
            skillSystem = system;
        }

        public void SetAttackTarget (GameObject obj, Action enterAction, Action exitAction) {
            target = obj;
            attackDistance = gameObject.GetComponent<InfoPart>().radio + target.GetComponent<InfoPart>().radio;
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void SetNotic(Action<SkillInfoPart> notic)
        {
            this.notic = notic;
        }

        public void Cast(SkillInfoPart skillInfo) {
            skillSystem.Cast(skillInfo.skillName, gameObject);
            notic(skillInfo);
        }

        public void Cast(SkillInfoPart skillInfo, GameObject target)
        {
            skillSystem.Cast(skillInfo.skillName, gameObject, target);
            notic(skillInfo);
        }

        public void Cast(SkillInfoPart skillInfo, Vector3 position)
        {
            skillSystem.Cast(skillInfo.skillName, gameObject, position);
            notic(skillInfo);
        }
    }
}
