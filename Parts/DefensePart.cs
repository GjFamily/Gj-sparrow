using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<Skill, bool> cureNotic;
        private Action<Skill, bool> injuredNotic;
        private Action<Skill> addNotic;
        private Action<Skill> delNotic;

        public void SetNotic(Action<Skill, bool> cure, Action<Skill, bool> injured, Action<Skill> addStatus, Action<Skill> delStatus)
        {
            cureNotic = cure;
            injuredNotic = injured;
            addNotic = addStatus;
            delNotic = delStatus;
        }

        public void BeCast(GameObject target, Skill skill)
        {
            switch (skill.skillType) {
                case SkillType.Injured:
                    if (injuredNotic != null) {
                        injuredNotic(skill, false);
                    }
                    break;
                case SkillType.Cure:
                    if (cureNotic != null) {
                        cureNotic(skill, false);
                    }
                    break;
            }
            if (skill.hasExtra) {
                if (Info.attr.statusList.Count == 0) {
                    InvokeRepeating(CHECK_STATUS, 1, 1);
                }
                if (addNotic != null) {
                    addNotic(skill);
                }
            }
        }

        private const string CHECK_STATUS = "CheckStatus";

        private void CheckStatus () {
            if (Info.attr.statusList.Count == 0)
            {
                CancelInvoke(CHECK_STATUS);
            }
            float time = Time.time;
            foreach(Status status in Info.attr.statusList) {
                switch (status.Skill.extra.extraType)
                {
                    case SkillExtraType.Injured:
                    case SkillExtraType.InjuredAndStatus:
                        if (injuredNotic != null)
                        {
                            injuredNotic(status.Skill, false);
                        }
                        break;
                    case SkillExtraType.Cure:
                    case SkillExtraType.CureAndStatus:
                        if (cureNotic != null)
                        {
                            cureNotic(status.Skill, false);
                        }
                        break;
                }
                if (status.time + status.Skill.extra.sustainedTime < time) {
                    if (delNotic != null) {
                        delNotic(status.Skill);
                    }
                }
            }
        }
    }
}