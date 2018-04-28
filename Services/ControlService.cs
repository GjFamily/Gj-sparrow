using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    public class ControlService
    {
        public static ControlService single;

        private Dictionary<string, JSONObject> targetMap = new Dictionary<string, JSONObject>();
        private Dictionary<string, Type> controlMap = new Dictionary<string, Type>();

        static ControlService()
        {
            single = new ControlService();
        }

        public void Init(Dictionary<string, Type> dict, JSONArray jSONArray)
        {
            controlMap = dict;
            SetTarget(jSONArray);
        }

        private void SetTarget(JSONArray jSONArray)
        {
            foreach (JSONObject json in jSONArray)
            {
                targetMap.Add(json["name"], json);
            }
        }

        public TargetAttr GetTarget(string targetName)
        {
            return new TargetAttr(targetMap[targetName]);
        }

        public BaseControl MakeControl(string targetName, Vector3 position)
        {
            return MakeControl(targetName, position, null);
        }

        public BaseControl MakeControl(string targetName, Vector3 position, GameObject master)
        {
            TargetAttr attr = GetTarget(targetName);
            BaseControl baseControl = ObjectService.single.MakeEmpty(controlMap[attr.name], attr.name, position) as BaseControl;
            baseControl.Init(attr, master);
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
