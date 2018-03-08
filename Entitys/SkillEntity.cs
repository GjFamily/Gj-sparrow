using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    public class SkillEntity : BaseEntity
    {
        public SkillInfo SkillInfo
        {
            get
            {
                return GetComponent<SkillInfo>();
            }
        }
        public ExtraInfo ExtraInfo
        {
            get
            {
                return GetComponent<ExtraInfo>();
            }
        }

        private Action beforeCast;
        private Action afterCast;
        private Action readyCast;
        private Action startCast;
        private Action endCast;
        private Action cancelCast;

        private bool waiting = false;
        private bool sustaining = false;

        protected Transform targetTransform;
        protected GameObject targetObj;

        public void SetMaster(GameObject obj)
        {
            Appear();
            GetComponent<BeLongPart>().SetMaster(obj);
            if (SkillInfo != null)
            {
                SkillInfo.master = obj;
            }
            if (ExtraInfo != null)
            {
                ExtraInfo.master = obj;
            }
        }

        public GameObject GetMaster()
        {
            return GetComponent<BeLongPart>().GetMaster();
        }

        public void Init(Action before, Action after, Action start, Action end, Action ready)
        {
            beforeCast = before;
            afterCast = after;
            startCast = start;
            endCast = end;
            readyCast = ready;
            if (SkillInfo.castType == SkillInfo.CastType.Now || SkillInfo.castType == SkillInfo.CastType.Sustained)
            {
                Start();
            }
            else
            {
                Ready();
            }
        }

        private void Ready()
        {
            readyCast();
            ReadyCast();
            waiting = true;
            Invoke("Start", SkillInfo.waitTime);
        }

        private void After()
        {
            afterCast();
        }

        private void End()
        {
            sustaining = false;
            endCast();
        }

        protected virtual void ReadyCast()
        {

        }

        protected virtual void Cast()
        {

        }

        public void CancelCast()
        {
            if (waiting)
            {
                CancelInvoke("Start");
                After();
            }
            else if (sustaining)
            {
                CancelInvoke("End");
                End();
            }
        }

        public void Start()
        {
            waiting = false;
            if (SkillInfo.castType == SkillInfo.CastType.ReadyAndSustained || SkillInfo.castType == SkillInfo.CastType.ReadyAndSustained)
            {
                startCast();
                Cast();
                Invoke("End", SkillInfo.sustainedTime);
            }
            else
            {
                beforeCast();
                Cast();
                sustaining = true;
                Invoke("After", SkillInfo.castTime);
            }
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

        protected void BeCastTarget(GameObject target)
        {
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
