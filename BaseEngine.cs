using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
    public class BaseEngine : MonoBehaviour
    {
        private Action<Skill> readyCast;
        private Action<Skill> startCast;
        private Action<Skill> endCast;
        private Action<Skill> cancelCast;

        private bool waiting = false;
        private bool sustaining = false;
        private bool auto = false;

        protected float power = 0;
        private float startTime = 0;

        protected Transform targetTransform;
        protected GameObject target;

        private GameObject master;
        private Skill skill;

        public void Init(GameObject obj, Skill s)
        {
            master = obj;
            skill = s;
        }

        public void Ignition(Action<Skill> start, Action<Skill> end, Action<Skill> ready, Action<Skill> cancel, bool b)
        {
            startCast = start;
            endCast = end;
            readyCast = ready;
            cancelCast = cancel;
            this.auto = b;
            if (skill.castType == CastType.Now || skill.castType == CastType.Sustained)
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
            readyCast(skill);
            waiting = true;
            if (auto) Invoke(READY_END, skill.readyTime);
        }

        private const string READY_END = "ReadyEnd";

        public void ReadyEnd()
        {
            power = (Time.time - startTime) / skill.readyTime;
            if (power > 1) power = 1;
            waiting = false;
            Now();
        }

        private const string END = "End";

        private void End()
        {
            sustaining = false;
            endCast(skill);
            EngineService.single.DestroyEngine(gameObject);
        }

        public void Cancel()
        {
            if (waiting)
            {
                if (auto) CancelInvoke(READY_END);
                waiting = false;
                cancelCast(skill);
            }
            else if (sustaining)
            {
                CancelInvoke(END);
                sustaining = false;
                cancelCast(skill);
            }
        }

        private const string CAST = "Cast";

        public void Now()
        {
            startCast(skill);
            Invoke(CAST, 0);
            if (skill.castType == CastType.Sustained || skill.castType == CastType.ReadyAndSustained)
            {
                sustaining = true;
                Invoke(END, skill.sustainedTime);
            }
            else
            {
                Invoke(END, skill.castTime);
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
            DefensePart defensePart = target.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                defensePart.BeCast(master, skill);
            }
        }
    }
}
