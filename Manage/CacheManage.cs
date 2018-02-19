using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class CacheManage
    {
        public static CacheManage single;

        private enum CacheType
        {
            Skill,
            Target,
            Other
        }

        private Dictionary<string, List<GameObject>> cache = new Dictionary<string, List<GameObject>>();

        static CacheManage()
        {
            single = new CacheManage();
        }

        private string GenerateKey(CacheType cacheType, string name)
        {
            string prefix = "";
            switch (cacheType)
            {
                case CacheType.Skill:
                    prefix = "s";
                    break;
                case CacheType.Target:
                    prefix = "s";
                    break;
                default:
                    prefix = "o";
                    break;
            }
            return prefix + "-" + name;
        }

        public void SetSkillCache(string skillName, GameObject obj)
        {
            string key = GenerateKey(CacheType.Skill, skillName);
            SetCache(key, obj);
        }

        public void SetTargetCache(string targetName, GameObject obj)
        {
            string key = GenerateKey(CacheType.Target, targetName);
            SetCache(key, obj);
        }

        public void SetOtherCache(string otherName, GameObject obj)
        {
            string key = GenerateKey(CacheType.Other, otherName);
            SetCache(key, obj);
        }

        public GameObject GetSkillCache(string skillName)
        {
            string key = GenerateKey(CacheType.Skill, skillName);
            return GetCache(key);
        }

        public GameObject GetTargetCache(string targetName)
        {
            string key = GenerateKey(CacheType.Target, targetName);
            return GetCache(key);
        }

        public GameObject GetOtherCache(string otherName)
        {
            string key = GenerateKey(CacheType.Other, otherName);
            return GetCache(key);
        }

        private GameObject GetCache(string key)
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

        private void SetCache (string key, GameObject obj) {
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, new List<GameObject>());
            }
            cache[key].Add(obj);
        }

    }
}
