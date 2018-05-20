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
        private float targetDistance;
        private float attackDistance;
        private float attackRadius = 0.3f;
        private bool attacking = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
                if (attacking) {
                    if (distance > targetDistance + attackRadius) {
                        if (exitAttackNotic != null)
                        {
                            exitAttackNotic();
                        }
                        attacking = false;
                    }
                } else {
                    if (distance < targetDistance + (attackRadius / 2)) {
                        if (enterAttackNotic != null)
                        {
                            enterAttackNotic();
                        }
                        attacking = true;
                    }
                }
            }
        }

        public void SetNotic(Action enterAction, Action exitAction) {
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void SetAttackTarget(GameObject obj)
        {
            target = obj;
            targetDistance = Info.attr.baseAttr.radius + CoreTools.GetInfo(obj).attr.baseAttr.radius;
        }
    }
}
