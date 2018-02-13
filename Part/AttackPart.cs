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
        private float attackDistance;
        private bool attacking = false;
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

        public void SetAttackTarget (GameObject obj, Action enterAction, Action exitAction) {
            target = obj;
            attackDistance = gameObject.GetComponent<InfoPart>().radio + target.GetComponent<InfoPart>().radio;

            Debug.LogFormat("{0}, {1}", target.name, attackDistance);
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void Launch(GameObject skill, GameObject start, float power, float distance, float speed) {
            Vector3 p = new Vector3(start.transform.position.x, 0, start.transform.position.z) + start.transform.forward * distance;
            SkillEntity skillEntity = skill.GetComponent<SkillEntity>();
            if (skillEntity!=null) {
                skillEntity.SetMaster(gameObject, power).Cast(start.transform.position, p, speed);
            }
        }

        public void Simple (GameObject skill, GameObject start, float power, float distance) {
            Vector3 p = new Vector3(start.transform.position.x, 0, start.transform.position.z) + start.transform.forward * distance;
            SkillEntity skillEntity = skill.GetComponent<SkillEntity>();
            if (skillEntity != null)
            {
                skillEntity.SetMaster(gameObject, power).Cast(p, distance);
            }
        }
    }
}
