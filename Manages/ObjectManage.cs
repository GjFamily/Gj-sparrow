using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class ObjectManage
    {
        public static ObjectManage single;

        private GameObject container;
        private Dictionary<string, GameObject> objMap = new Dictionary<string, GameObject>();
        private Dictionary<string, SkillInfo> skillInfoMap = new Dictionary<string, SkillInfo>();

        static ObjectManage()
        {
            single = new ObjectManage();
        }

        public void SetContainer (GameObject obj) {
            container = obj;
        }

        public void SetObjs(GameObject[] objs) {
            foreach (GameObject obj in objs)
            {
                objMap.Add(obj.name, obj);
            }
        }

        public GameObject GetObj(string objName)
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
            GameObject obj = ObjManage.single.GetObj(objName);
            if (obj != null)
            {
                return ModelTools.Create(obj, container);
            }
            else
            {
                return null;
            }
        }

        public GameObject MakeObj(string objName)
        {
            GameObject obj = CacheManage.single.GetCache(objName);
            if (obj == null)
            {
                obj = CreateObj(objName);
            }
            return obj;
        }
    }
}
