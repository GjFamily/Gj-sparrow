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
                targetMap.Add(json[OBJECTATTR.NAME], json);
            }
        }

        public ObjectAttr GetTarget(string targetName)
        {
            return new ObjectAttr(targetMap[targetName]);
        }

        public BaseControl MakeControl(string targetName, ObjectControl control, Vector3 position, Quaternion rotation)
        {
            return MakeControl(targetName, control, position, rotation, null);
        }

        public BaseControl MakeControl(string targetName, ObjectControl control, Vector3 position, Quaternion rotation, GameObject master)
        {
            ObjectAttr attr = GetTarget(targetName);
            BaseControl baseControl = ObjectService.single.MakeEmpty(controlMap[attr.name], attr.name, position, rotation) as BaseControl;
            baseControl.Init(attr, control, master);
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
