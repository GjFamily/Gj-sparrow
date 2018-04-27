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

        public void SetNotic(Action<Skill> ready, Action<Skill> start, Action<Skill> end, Action<Skill> cancel)
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

        public void Cast(string skillName)
        {
            Skill skill = EngineService.single.GetSkill(skillName);
            if (inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                Cast(engine);
            }
        }

        public void Cast(string skillName, GameObject target)
        {
            Skill skill = EngineService.single.GetSkill(skillName);
            if (CoreTools.AllowTarget(skill, gameObject, target) && CoreTools.AllowRange(skill, gameObject, target) && inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                engine.Set(target);
                Cast(engine);
            }
        }

        public void Cast(string skillName, Transform transform)
        {
            Skill skill = EngineService.single.GetSkill(skillName);
            if (CoreTools.AllowRange(skill, gameObject, transform) && inspect != null && inspect(skill))
            {
                BaseEngine baseEngine = EngineService.single.MakeEngine(gameObject, skill);
                engine.Set(transform);
                Cast(engine);
            }
        }

        public void Cast(BaseEngine baseEngine)
        {
            engine = baseEngine;
            engine.Ignition(startCast, endCast, readyCast, cancelCast, GetAttribute("auto") > 0);
        }
    }
}
