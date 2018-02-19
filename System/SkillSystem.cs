using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Gj
{
    public class SkillSystem : BaseSystem
    {
        public GameObject content;
        private Dictionary<string, GameObject> skillMap;

        public GameObject GetSkill(string skillName)
        {
            if (skillMap.ContainsKey(skillName))
            {
                return skillMap[skillName];
            }
            else
            {
                return null;
            }
        }

        public SkillInfoPart GetSkillInfo(string skillName)
        {
            GameObject skillObj = GetSkill(skillName);
            if (skillObj != null)
            {
                return skillObj.GetComponent<SkillInfoPart>();
            }
            else
            {
                return null;
            }
        }

        private GameObject CreateSkill(string skillName)
        {
            GameObject skill = GetSkill(skillName);
            if (skill != null)
            {
                return ModelTools.Create(skill, content);
            }
            else
            {
                return null;
            }
        }

        private GameObject GetSkillObj(string skillName)
        {
            GameObject obj = CacheManage.single.GetSkillCache(skillName);
            if (obj == null)
            {
                obj = CreateSkill(skillName);
            }
            return obj;
        }

        private SkillEntity InitSkill(string skillName, GameObject master)
        {
            GameObject obj = GetSkillObj(skillName);
            if (obj != null)
            {
                SkillEntity skill = obj.GetComponent<SkillEntity>();
                if (skill != null)
                {
                    skill.SetMaster(master);
                    return skill;
                }
                else
                {
                    Destroy(obj);
                }
            }
            return null;
        }

        public void Cast(string skillName, GameObject master)
        {
            SkillEntity skill = InitSkill(skillName, master);
            if (skill != null)
            {
                skill.Cast();
            }
        }

        public void Cast(string skillName, GameObject master, GameObject target)
        {
            SkillEntity skill = InitSkill(skillName, master);
            if (skill != null)
            {
                skill.Cast(target);
            }
        }

        public void Cast(string skillName, GameObject master, Vector3 position)
        {
            SkillEntity skill = InitSkill(skillName, master);
            if (skill != null)
            {
                skill.Cast(position);
            }
        }
    }
}
