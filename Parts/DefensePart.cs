using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<GameObject, Skill, bool> cureNotic;
        private Action<GameObject, Skill, bool> injuredNotic;
        private Action<int, int> statusNotic;

        public void SetNotic(Action<GameObject, Skill, bool> cure, Action<GameObject, Skill, bool> injured, Action<int, int> status)
        {
            cureNotic = cure;
            injuredNotic = injured;
            statusNotic = status;
        }

        public void BeCast(GameObject target, Skill skill)
        {
            switch (skill.skillType)
            {
                case SkillType.Injured:
                    if (injuredNotic != null)
                    {
                        injuredNotic(target, skill, false);
                    }
                    break;
                case SkillType.Cure:
                    if (cureNotic != null)
                    {
                        cureNotic(target, skill, false);
                    }
                    break;
            }
            if (skill.hasExtra)
            {
                if (Info.attr.statusList.Count == 0)
                {
                    InvokeRepeating(CHECK_STATUS, 1, 1);
                }
                int id = skill.id;
                int index = -1;
                for (int i = 0; i < Info.attr.statusList.Count; i++)
                {
                    Status? status = Info.attr.statusList[i];
                    if (status != null && status.Value.skill.id == skill.id) {
                        index = i;
                    }
                    if (index < 0 && status == null)
                    {
                        index = i;
                    }
                }
                if (index < 0)
                {
                    index = Info.attr.statusList.Count;
                }
                if (statusNotic != null)
                {
                    statusNotic(index, id);
                }
            }
        }

        public void UpdateStatus(int index, float time, float value)
        {
            int id = (int)value;
            if (id > 0)
            {
                Status status = new Status
                {
                    time = time,
                    skill = EngineService.single.GetSkill(id)
                };
                if (index > Info.attr.statusList.Count - 1)
                {
                    Info.attr.statusList.Insert(index, status);
                } else {
                    Info.attr.statusList[index] = status;
                }
            }else {
                if (index <= Info.attr.statusList.Count - 1)
                {
                    Info.attr.statusList[index] = null;
                }
            }

        }

        private const string CHECK_STATUS = "CheckStatus";

        private void CheckStatus()
        {
            if (Info.attr.statusList.Count == 0)
            {
                CancelInvoke(CHECK_STATUS);
            }
            float time = Time.time;
            for (int i = 0; i < Info.attr.statusList.Count; i++)
            {
                if (Info.attr.statusList[i] == null) continue;
                Status status = Info.attr.statusList[i].Value;
                if (status.time + status.skill.extra.sustainedTime < time)
                {
                    if (statusNotic != null)
                    {
                        statusNotic(i, 0);
                    }
                }
                else
                {
                    switch (status.skill.extra.extraType)
                    {
                        case SkillExtraType.Injured:
                        case SkillExtraType.InjuredAndStatus:
                            if (injuredNotic != null)
                            {
                                injuredNotic(null, status.skill, false);
                            }
                            break;
                        case SkillExtraType.Cure:
                        case SkillExtraType.CureAndStatus:
                            if (cureNotic != null)
                            {
                                cureNotic(null, status.skill, false);
                            }
                            break;
                    }
                }
            }
        }
    }
}