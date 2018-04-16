using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class ObjectService
    {
        public static ObjectService single;

        private Dictionary<string, GameObject> objMap = new Dictionary<string, GameObject>();
        private Dictionary<string, SkillInfo> skillInfoMap = new Dictionary<string, SkillInfo>();

        static ObjectService()
        {
            single = new ObjectService();
        }

        public void SetObjs(GameObject[] objs)
        {
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
            GameObject obj = ObjectService.single.GetObj(objName);
            if (obj != null)
            {
                return ModelTools.Create(obj);
            }
            else
            {
                return null;
            }
        }

        public GameObject MakeObj(string objName)
        {
            return MakeObj(objName, null);
        }

        public GameObject MakeObj(string objName, GameObject parent)
        {
            GameObject obj = CacheService.single.GetCache(objName);
            if (obj == null)
            {
                obj = CreateObj(objName);
            }
            if (obj != null && parent != null)
            {
                obj.transform.parent = parent.transform;
            }
            return obj;
        }

        public T MakeObj<T>(string objName, GameObject parent, Vector3 position) where T : Component
        {
            GameObject obj = CacheService.single.GetCache(objName);
            if (obj == null)
            {
                obj = ModelTools.Create(null);
            }
            if (obj != null && parent != null)
            {
                obj.transform.parent = parent.transform;
                obj.transform.position = position;
            }
            return obj.AddComponent<T>();
        }

        public void DestroyObj(GameObject obj)
        {
            obj.transform.parent = null;
            CacheService.single.SetCache(obj.name, obj);
        }
    }
}
