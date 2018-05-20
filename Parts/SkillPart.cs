using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class SkillPart : BasePart
    {
        private Action<Skill> consume;
        private Func<Skill, bool> inspect;

        private BaseEngine engine;

        private Action<Skill> cancelCast;
        private Action<Skill> readyCast;
        private Action<Skill> startCast;
        private Action<Skill> endCast;

        public void SetPower(Func<Skill, bool> func, Action<Skill> action)
        {
            inspect = func;
            consume = action;
        }

        public void SetNotic(Action<Skill> start, Action<Skill> end, Action<Skill> ready, Action<Skill> cancel)
        {
            startCast = start;
            endCast = end;
            readyCast = ready;
            cancelCast = cancel;
        }

        public void OkCast()
        {
            if (engine != null)
            {
                engine.ReadyEnd();
            }
        }

        public void CancelCast()
        {
            if (engine != null)
            {
                engine.Cancel();
            }
        }

        public void Cast(Skill skill)
        {
            if (inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                Cast(skill, baseEngine);
            }
        }

        public void Cast(Skill skill, GameObject target)
        {
            if (CoreTools.AllowTarget(skill, gameObject, target) && CoreTools.AllowRange(skill, gameObject, target) && inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                baseEngine.Set(target);
                Cast(skill, baseEngine);
            }
        }

        public void Cast(Skill skill, Transform transform)
        {
            if (CoreTools.AllowRange(skill, gameObject, transform) && inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                baseEngine.Set(transform);
                Cast(skill, baseEngine);
            }
        }

        public void Cast(Skill skill, BaseEngine baseEngine)
        {
            engine = baseEngine;
            engine.Ignition(startCast, endCast, readyCast, cancelCast, Info.attr.auto);
            consume(skill);
        }
    }
}
