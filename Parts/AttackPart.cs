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

        public void SetNotic(Action enterAction, Action exitAction) {
            enterAttackNotic = enterAction;
            exitAttackNotic = exitAction;
        }

        public void SetAttackTarget(GameObject obj)
        {
            target = obj;
            attackDistance = GetAttribute("radio") + CoreTools.GetAttribute(obj, "radio");
            attackDistance += attackDistance * 0.3f;
        }
    }
}
