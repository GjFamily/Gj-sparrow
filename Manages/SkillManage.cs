using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class SkillManage
    {
        public static SkillManage single;

        private Dictionary<string, SkillInfo> skillInfoMap = new Dictionary<string, SkillInfo>();

        static SkillManage()
        {
            single = new SkillManage();
        }

        public SkillInfo GetSkillInfo(string skillName)
        {
            if (skillInfoMap.ContainsKey(skillName))
            {
                return skillInfoMap[skillName];
            }
            GameObject skillObj = ObjectManage.single.GetObj(skillName);
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
            GameObject obj = ObjectManage.single.MakeObj(skillName);
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
