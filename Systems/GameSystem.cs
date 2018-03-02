using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class GameSystem : BaseSystem
    {
        public GameObject content;

        public GameObject[] objs;
        private Dictionary<string, GameObject> objMap = new Dictionary<string, GameObject>();

        protected override void Awake()
        {
            base.Awake();
            foreach (GameObject obj in objs)
            {
                objMap.Add(obj.name, obj);
            }
        }

        private GameObject GetObj(string objName)
        {
            if (objMap.ContainsKey(objName))
            {
                return objMap[objName];
            }
            else
            {
                return null;
            }
        }

        private GameObject CreateObj(string objName)
        {
            GameObject obj = GetObj(objName);
            if (obj != null)
            {
                return ModelTools.Create(obj, content);
            }
            else
            {
                return null;
            }
        }

        protected GameObject MakeObj(string objName)
        {
            GameObject obj = CacheManage.single.GetCache(objName);
            if (obj == null)
            {
                obj = CreateObj(objName);
            }
            return obj;
        }

        public GameObject MakeTarget(string targetName)
        {
            GameObject targetObj = MakeObj(targetName);
            if (targetObj != null)
            {
                targetObj.GetComponent<TargetEntity>().SetGameSystem(this);
                return targetObj;
            }
            else
            {
                return null;
            }
        }

        public SkillInfo GetSkillInfo(string skillName)
        {
            GameObject skillObj = GetObj(skillName);
            if (skillObj != null)
            {
                return skillObj.GetComponent<SkillInfo>();
            }
            else
            {
                return null;
            }
        }

        public SkillEntity InitSkill(string skillName, GameObject master)
        {
            GameObject obj = MakeObj(skillName);
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

        public void Cast(string skillName, GameObject master, Transform transform)
        {
            SkillEntity skill = InitSkill(skillName, master);
            if (skill != null)
            {
                skill.Cast(transform);
            }
        }
    }
}

