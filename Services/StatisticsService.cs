using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class StatisticsService
    {
        public static StatisticsService single;

        private Dictionary<string, float> store;

        private Dictionary<string, List<Notic>> noticDic;

        private struct Notic
        {
            public Action<float> notic;
            public Action<string, float> cateNotic;
        }

        static StatisticsService()
        {
            single = new StatisticsService();
        }

        public void Start()
        {
            store = new Dictionary<string, float>();
            noticDic = new Dictionary<string, List<Notic>>();
        }

        public void OnChange(string type, string category, Action<float> action)
        {
            OnChange(type, new Notic { notic = action });
        }

        public void OnChange(string type, Action<string, float> action)
        {
            OnChange(type, new Notic { cateNotic = action });
        }

        private void OnChange(string key, Notic n)
        {
            if (!noticDic.ContainsKey(key))
            {
                noticDic.Add(key, new List<Notic>());
            }
            noticDic[key].Add(n);
        }

        public void Record(GameObject player, GameObject target, string type, string category, float value)
        {
            Debug.LogFormat("{0} -> {1}: {2} {3} {4}", player.name, target.name, type, category, value);
            SaveLog(type, category, value);
        }

        public void Event(GameObject player, string type, string category, float value)
        {
            Debug.LogFormat("{0}: {1} {2} {3}", player.name, type, category, value);
            SaveLog(type, category, value);
        }

        private void SaveLog(string type, string category, float value)
        {
            SaveLog(type, value);
            SaveLog(GetKey(type, category), value);

            Broadcast(type, category, value);
        }

        private void SaveLog(string key, float value) {
            if (!store.ContainsKey(key))
            {
                store.Add(key, value);
            }
            else
            {
                store[key] = store[key] + value;
            }
        }

        private List<Notic> GetNoticList(string type)
        {
            if (noticDic.ContainsKey(type))
            {
                return noticDic[type];
            }
            else
            {
                return null;
            }
        }

        private void Broadcast(string type, string category, float value)
        {
            List<Notic> noticList = GetNoticList(type);
            if (noticList != null)
            {
                foreach (Notic n in noticList)
                {
                    if (n.cateNotic != null)
                    {
                        n.cateNotic(category, value);
                    }
                    else
                    {
                        noticList.Remove(n);
                    }
                }
            }
            if (category != null)
            {
                noticList = GetNoticList(GetKey(type, category));
                if (noticList != null)
                {
                    foreach (Notic n in noticList)
                    {
                        if (n.notic != null)
                        {
                            n.notic(value);
                        }
                        else
                        {
                            noticList.Remove(n);
                        }
                    }
                }
            }
        }

        public float LoadLog(string type, string category)
        {
            if (type != null && category != null)
            {
                return LoadLog(GetKey(type, category));
            }
            else
            {
                return 0;
            }
        }

        public float LoadLog(string key)
        {
            if (store.ContainsKey(key))
            {
                return store[key];
            }
            else
            {
                return 0;
            }
        }

        private string GetKey(string type, string category)
        {
            return type + "-" + category;
        }
    }
}