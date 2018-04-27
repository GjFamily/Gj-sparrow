using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    // TODO 
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
            SetEngine(dict);
            SetSkill(jSONArray);
        }

        private void SetSkill(JSONArray jSONArray)
        {
            foreach (JSONObject json in jSONArray)
            {
                skillMap.Add(json["name"], FormatSkill(json));
            }
        }

        private void SetEngine(Dictionary<string, Type> dict)
        {
            engineMap = dict;
        }

        private Skill FormatSkill(JSONObject json)
        {
            return new Skill
            {
                name = json["name"],
                value = json["vaule"].AsFloat,
                need = json["need"].AsFloat,
                range = json["rang"].AsFloat,
                type = engineMap[json["name"]],
                readyTime = json["readyTime"].AsFloat,
                castTime = json["castTime"].AsFloat,
                intervalTime = json["intervalTime"].AsFloat,
                sustainedTime = json["sustainedTime"].AsFloat,
                targetRelation = (TargetRelation)json["targetRelation"].AsInt,
                targetNum = (TargetNum)json["targetNum"].AsInt,
                targetNeed = (TargetNeed)json["targetNeed"].AsInt,
                skillType = (SkillType)json["skillType"].AsInt,
                castType = (CastType)json["castType"].AsInt,
                needType = (NeedType)json["needType"].AsInt
            };
        }

        public Skill GetSkill(string skillName)
        {
            return skillMap[skillName];
        }

        public BaseEngine MakeEngine(GameObject obj, Skill skill)
        {
            BaseEngine baseEngine = ObjectService.single.MakeObj(skill.type, skill.name, obj.transform.position) as BaseEngine;
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
