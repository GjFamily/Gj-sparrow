using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class CacheManage
    {
        public static CacheManage single;

        private Dictionary<string, List<GameObject>> cache = new Dictionary<string, List<GameObject>>();

        static CacheManage()
        {
            single = new CacheManage();
        }

        public GameObject GetCache(string key)
        {
            if (cache.ContainsKey(key))
            {
                List<GameObject> list = cache[key];
                GameObject obj = null;
                if (list.Count > 0) {
                    obj = list[0];
                    list.RemoveAt(0);
                }
                return obj;
            }
            else
            {
                return null;
            }
        }

        public void SetCache (string key, GameObject obj) {
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, new List<GameObject>());
            }
            cache[key].Add(obj);
        }

    }
}
