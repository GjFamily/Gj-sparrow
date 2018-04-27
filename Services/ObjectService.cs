using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class ObjectService
    {
        public static ObjectService single;

        private GameObject container;

        private Dictionary<string, GameObject> objMap = new Dictionary<string, GameObject>();

        static ObjectService()
        {
            single = new ObjectService();
        }

        public void SetContainer(GameObject obj)
        {
            container = obj;
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
            if (parent != null)
            {
                obj.transform.SetParent(parent.transform, false);
            }
            return obj;
        }

        public Component MakeObj(Type type, string objName, Vector3 position)
        {
            GameObject obj = MakeObj(objName);
            if (obj != null)
            {
                obj.transform.SetParent(container.transform, false);
                obj.transform.position = position;
            }
            return obj.AddComponent(type);
        }

        public Component MakeEmpty(Type type, string objName, Vector3 position)
        {
            GameObject obj = CacheService.single.GetCache(objName);
            if (obj == null)
            {
                obj = ModelTools.Create(null);
            }
            if (obj != null)
            {
                obj.transform.SetParent(container.transform, false);
                obj.transform.position = position;
            }
            obj.name = objName;
            return obj.AddComponent(type);
        }

        public T MakeTarget<T>(string objName, Vector3 position) where T : Component
        {
            GameObject obj = CacheService.single.GetCache(objName);
            if (obj == null)
            {
                obj = ModelTools.Create(null);
            }
            if (obj != null)
            {
                obj.transform.SetParent(container.transform, false);
                obj.transform.position = position;
            }
            obj.name = objName;
            return obj.AddComponent<T>();
        }

        public void DestroyObj(GameObject obj)
        {
            obj.transform.parent = null;
            CacheService.single.SetCache(obj.name, obj);
        }
    }
}
