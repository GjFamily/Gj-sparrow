using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    public class EngineService
    {
        public static EngineService single;

        private Dictionary<string, int> skillMap = new Dictionary<string, int>();
        private List<Skill> skillList = new List<Skill>();
        private Dictionary<string, Type> engineMap = new Dictionary<string, Type>();

        static EngineService()
        {
            single = new EngineService();
        }

        public void Init(Dictionary<string, Type> dict, JSONArray jSONArray)
        {
            engineMap = dict;
            SetSkill(jSONArray);
        }

        private void SetSkill(JSONArray jSONArray)
        {
            foreach (JSONObject json in jSONArray)
            {
                skillList.Add(new Skill(json));
                skillMap.Add(json[SKILL.NAME], skillList.Count - 1);
            }
        }

        public Skill GetSkill(string skillName)
        {
            return GetSkill(skillMap[skillName]);
        }

        public Skill GetSkill(int id) {
            return skillList[id];
        }

        public BaseEngine MakeEngine(GameObject obj, Skill skill)
        {
            BaseEngine baseEngine = ObjectService.single.MakeObj(engineMap[skill.name], skill.name, obj.transform.position, obj.transform.rotation) as BaseEngine;
            baseEngine.Init(obj, skill);
            baseEngine.gameObject.SetActive(true);
            return baseEngine;
        }

        public void DestroyEngine(GameObject obj)
        {
            obj.SetActive(false);
            ObjectService.single.DestroyObj(obj);
        }
    }
}
