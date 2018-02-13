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

        public virtual void Cast(Vector3 from, Vector3 to)
        {

        }

        public virtual void Cast(Vector3 position, float radius)
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

        protected bool IsSelf(GameObject obj)
        {
            if (Tools.GetMaster(obj) == Tools.GetMaster(gameObject))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool AllowAttack(GameObject obj)
        {
            RelationPart relation = Tools.GetMaster(gameObject).GetComponent<RelationPart>();
            if (relation != null)
            {
                return relation.IsEnemy(obj);
            }
            else
            {
                return false;
            }
        }
    }
}
