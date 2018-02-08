using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    public class SkillEntity : BaseEntity
    {
        protected float power;

        public SkillEntity SetMaster(GameObject obj, float power)
        {
            GetComponent<BeLongPart>().SetMaster(obj);
            this.power = power;
            return this;
        }

        public virtual void Cast(Vector3 from, Vector3 to, float speed)
        {

        }

        protected void AttackTarget(GameObject target)
        {
            DefensePart defensePart = target.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                defensePart.BeAttacked(power);
            }
        }

        protected bool IsSelf(GameObject obj) {
            if (GetMaster(obj) == GetMaster(gameObject))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool IsSkill(GameObject obj)
        {
            if (obj.GetComponent<SkillEntity>() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool IsTarget(GameObject obj)
        {
            if (obj.GetComponent<TargetEntity>() != null || obj.GetComponent<BeLongPart>() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
