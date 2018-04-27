using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    // TODO 
    public class ControlService
    {
        public static ControlService single;

        private Dictionary<string, Target> targetMap = new Dictionary<string, Target>();
        private Dictionary<string, Type> controlMap = new Dictionary<string, Type>();

        static ControlService()
        {
            single = new ControlService();
        }

        public void Init(Dictionary<string, Type> dict, JSONArray jSONArray) {
            SetControl(dict);
            SetTarget(jSONArray);
        }

        private void SetTarget(JSONArray jSONArray)
        {
            foreach (JSONObject json in jSONArray)
            {
                targetMap.Add(json["name"], FormatTarget(json));
            }
        }

        private void SetControl(Dictionary<string, Type> dict)
        {
            controlMap = dict;
        }

        private Target FormatTarget(JSONObject json)
        {
            //return new Skill
            //{
            //    name = json["name"],
            //    value = json["vaule"].AsFloat,
            //    need = json["need"].AsFloat,
            //    range = json["rang"].AsFloat,
            //    type = controlMap[json["name"]],
            //    readyTime = json["readyTime"].AsFloat,
            //    castTime = json["castTime"].AsFloat,
            //    intervalTime = json["intervalTime"].AsFloat,
            //    sustainedTime = json["sustainedTime"].AsFloat,
            //    targetRelation = (TargetRelation)json["targetRelation"].AsInt,
            //    targetNum = (TargetNum)json["targetNum"].AsInt,
            //    targetNeed = (TargetNeed)json["targetNeed"].AsInt,
            //    skillType = (SkillType)json["skillType"].AsInt,
            //    castType = (CastType)json["castType"].AsInt,
            //    needType = (NeedType)json["needType"].AsInt
            //};
            return new Target
            {

            };
        }

        public BaseControl MakeControl(Target target, Vector3 position)
        {
            return MakeControl(target, position, null);
        }

        public BaseControl MakeControl(Target target, Vector3 position, GameObject master)
        {
            BaseControl baseControl = ObjectService.single.MakeEmpty(target.type, target.name, position) as BaseControl;
            baseControl.Init(target, master);
            baseControl.gameObject.SetActive(true);
            return baseControl;
        }

        public void DestroyControl(GameObject obj)
        {
            obj.SetActive(false);
            ObjectService.single.DestroyObj(obj);
        }
    }
}
