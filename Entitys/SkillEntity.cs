using UnityEngine;
using System.Collections;
using System;

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

        private Action beforeCast;
        private Action afterCast;
        private Action readyCast;
        private Action startCast;
        private Action endCast;

        protected Transform targetTransform;
        protected GameObject targetObj;

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

        public void Ready (Action before, Action after, Action start, Action end, Action ready) {
            beforeCast = before;
            afterCast = after;
            startCast = start;
            endCast = end;
            readyCast = ready;
            if (SkillInfo.castType == SkillInfo.CastType.Now || SkillInfo.castType == SkillInfo.CastType.Sustained) {
                Cast();
            } else {
                readyCast();
            }

        }

        public virtual void ReadyCast () {
            
        }

        public virtual void CancelCast () {
            
        }

        public virtual void Cast()
        {

        }

        public void Set(GameObject target)
        {
            targetObj = target;
        }

        public void Set(Transform transform)
        {
            targetTransform = transform;
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
