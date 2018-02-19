using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    [RequirePart(typeof(SkillInfoPart))]
    public class SkillEntity : BaseEntity
    {
        public SkillInfoPart SkillInfo {
            get {
                return GetComponent<SkillInfoPart>();
            }
        }
        public void SetMaster(GameObject obj)
        {
            GetComponent<BeLongPart>().SetMaster(obj);
        }

        public GameObject GetMaster () {
            return GetComponent<BeLongPart>().GetMaster();
        }

        public virtual void Cast()
        {

        }

        public virtual void Cast(GameObject target)
        {

        }

        public virtual void Cast(Vector3 position)
        {

        }

        protected void AttackTarget(GameObject target)
        {
            DefensePart defensePart = target.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                defensePart.BeAttacked(SkillInfo, Tools.GetMaster(gameObject));
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
