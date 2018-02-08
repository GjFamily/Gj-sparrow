using UnityEngine;
using System.Collections;

namespace Gj
{
    public class SkillEntity : BaseEntity
    {
        protected GameObject master;
        protected float power;

        public SkillEntity SetMaster(GameObject obj, float power)
        {
            master = obj;
            this.power = power;
            return this;
        }

        public virtual void Cast(Vector3 from, Vector3 to, float speed)
        {
            
        }

        protected void AttackTarget(GameObject target) {
            DefensePart defensePart = target.GetComponent<DefensePart>();
            if (defensePart != null) {
                defensePart.BeAttacked(power);
            }
        }
    }
}
