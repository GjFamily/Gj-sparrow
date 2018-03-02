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
            Appear();
            GetComponent<BeLongPart>().SetMaster(obj);
            if (SkillInfo != null) {
                SkillInfo.master = obj;
            }
            if (ExtraInfo != null) {
                ExtraInfo.master = obj;
            }
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

        protected void CastTarget(GameObject target)
        {
            BeCastTarget(target);
            AddExtraTarget(target);
        }

        protected void BeCastTarget(GameObject target) {
            DefensePart defensePart = target.GetComponent<DefensePart>();
            if (defensePart != null && SkillInfo != null)
            {
                defensePart.BeCast(SkillInfo);
            }
        }

        protected void AddExtraTarget(GameObject target)
        {
            StatusPart statusPart = target.GetComponent<StatusPart>();
            if (statusPart != null && ExtraInfo != null)
            {
                statusPart.AddExtra(ExtraInfo);
            }
        }

        protected void CancelExtraTarget(GameObject target)
        {
            StatusPart statusPart = target.GetComponent<StatusPart>();
            if (statusPart != null && ExtraInfo != null)
            {
                statusPart.CancelExtra(ExtraInfo);
            }
        }
    }
}
