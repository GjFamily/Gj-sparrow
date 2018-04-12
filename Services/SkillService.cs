using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class SkillService
    {
        public static SkillService single;

        private Dictionary<string, SkillInfo> skillInfoMap = new Dictionary<string, SkillInfo>();

        static SkillService()
        {
            single = new SkillService();
        }

        public SkillInfo GetSkillInfo(string skillName)
        {
            if (skillInfoMap.ContainsKey(skillName))
            {
                return skillInfoMap[skillName];
            }
            GameObject skillObj = ObjectService.single.GetObj(skillName);
            if (skillObj != null)
            {
                SkillInfo skillInfo = skillObj.GetComponent<SkillInfo>();
                skillInfoMap.Add(skillName, skillInfo);
                return skillInfo;
            }
            else
            {
                return null;
            }
        }

        public SkillEntity InitSkill(string skillName, GameObject master)
        {
            GameObject obj = ObjectService.single.MakeObj(skillName);
            if (obj != null)
            {
                SkillEntity skill = obj.GetComponent<SkillEntity>();
                if (skill != null)
                {
                    skill.SetMaster(master);
                    return skill;
                }
            }
            return null;
        }
    }
}
