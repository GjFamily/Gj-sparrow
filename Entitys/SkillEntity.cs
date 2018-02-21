using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    public class SkillEntity : BaseEntity
    {
        public SkillInfo SkillInfo {
            get {
                return GetComponent<SkillInfo>();
            }
        }
        public ExtraInfo ExtraInfo {
            get {
                return GetComponent<ExtraInfo>();
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

        public virtual void Cast(Transform transform)
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

        protected void AddExtraTarget(GameObject target)
        {
            StatusPart statusPart = target.GetComponent<StatusPart>();
            if (statusPart != null)
            {
                statusPart.AddExtra(ExtraInfo, Tools.GetMaster(gameObject));
            }
        }

        protected void CancelExtraTarget(GameObject target)
        {
            StatusPart statusPart = target.GetComponent<StatusPart>();
            if (statusPart != null)
            {
                statusPart.CancelExtra(ExtraInfo, Tools.GetMaster(gameObject));
            }
        }
    }
}
