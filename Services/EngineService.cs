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

        private Dictionary<string, Skill> skillMap = new Dictionary<string, Skill>();
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
                skillMap.Add(json[SKILL.NAME], new Skill(json));
            }
        }

        public Skill GetSkill(string skillName)
        {
            return skillMap[skillName];
        }

        public BaseEngine MakeEngine(GameObject obj, Skill skill)
        {
            BaseEngine baseEngine = ObjectService.single.MakeObj(engineMap[skill.name], skill.name, obj.transform.position) as BaseEngine;
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
