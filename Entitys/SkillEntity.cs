using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
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
        private bool auto = false;

        protected float power = 0;
        private float startTime = 0;

        protected Transform targetTransform;
        protected GameObject target;

        public void SetMaster(GameObject obj)
        {
            if (SkillInfo != null)
            {
                SkillInfo.master = obj;
            }
            if (ExtraInfo != null)
            {
                ExtraInfo.master = obj;
            }
            Appear();
        }

        public void Init(Action before, Action after, Action start, Action end, Action ready, bool auto)
        {
            Appear();
            beforeCast = before;
            afterCast = after;
            startCast = start;
            endCast = end;
            readyCast = ready;
            this.auto = auto;
            if (SkillInfo.castType == SkillInfo.CastType.Now || SkillInfo.castType == SkillInfo.CastType.Sustained)
            {
                Now();
            }
            else
            {
                Ready();
            }
        }

        private void Ready()
        {
            power = 0;
            startTime = Time.time;
            readyCast();
            waiting = true;
            if (auto) Invoke("ReadyEnd", SkillInfo.readyTime);
        }

        public void ReadyEnd() {
            if (auto) CancelInvoke("ReadyEnd");
            power = (Time.time - startTime) / SkillInfo.readyTime;
            if (power > 1) power = 1;
            waiting = false;
            Now();
        }

        private void After()
        {
            waiting = false;
            afterCast();
        }

        private void End()
        {
            sustaining = false;
            endCast();
        }

        protected virtual void Cast()
        {

        }

        public void Cancel()
        {
            if (waiting)
            {
                if (auto) CancelInvoke("ReadyEnd");
                After();
            }
            else if (sustaining)
            {
                CancelInvoke("End");
                End();
            }
        }

        public void Now()
        {
            if (SkillInfo.castType == SkillInfo.CastType.Sustained || SkillInfo.castType == SkillInfo.CastType.ReadyAndSustained)
            {
                startCast();
                Cast();
                sustaining = true;
                Invoke("End", SkillInfo.sustainedTime);
            }
            else
            {
                beforeCast();
                Cast();
                Invoke("After", SkillInfo.castTime);
            }
        }

        public void Set(GameObject obj)
        {
            target = obj;
        }

        public void Set(Transform transform)
        {
            targetTransform = transform;
        }

        protected void CastTarget(GameObject target)
        {
            if (SkillInfo && SkillInfo.AllowTarget(target)) {
                BeCastTarget(target);
            }
            if (ExtraInfo && ExtraInfo.AllowTarget(target)) {
                AddExtraTarget(target);
            }
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
    }
}
