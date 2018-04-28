using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<GameObject, Skill> hitNotic;
        private Action<GameObject, Skill> dieNotic;

        public void SetNotic(Action<GameObject, Skill> die, Action<GameObject, Skill> hit)
        {
            dieNotic = die;
            hitNotic = hit;
        }

        public void BeCast(GameObject target, Skill skill)
        {
            float health = Info.attr.health;
            if (skill.value < 0)
            {
                if (hitNotic != null)
                {
                    hitNotic(target, skill);
                }
            }
            health += skill.value;
            if (health <= 0)
            {
                if (dieNotic != null)
                {
                    dieNotic(target, skill);
                }
                health = 0;
            }
            Info.attr.health = health;
        }
    }
}