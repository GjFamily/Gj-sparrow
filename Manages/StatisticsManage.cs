using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class StatisticsManage
    {
        public static StatisticsManage single;

        private Dictionary<string, float> store;

        private Dictionary<string, List<Action<float>>> notic;

        static StatisticsManage()
        {
            single = new StatisticsManage();
        }

        public void Start()
        {
            store = new Dictionary<string, float>();
            notic = new Dictionary<string, List<Action<float>>>();
        }

        public void OnChange(string type, string category, Action<float> action)
        {
            string key = GetKey(type, category);
            OnChange(key, action);
        }

        public void OnChange(string key, Action<float> action)
        {
            if (!notic.ContainsKey(key))
            {
                notic.Add(key, new List<Action<float>>());
            }
            notic[key].Add(action);
            action(LoadLog(key));
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
            if (type != null && category != null)
            {
                SaveLog(GetKey(type, category), value);
            }
            if (type != null)
            {
                SaveLog(type, value);
            }
        }

        private void Broadcast(string key, float value)
        {
            if (notic.ContainsKey(key))
            {
                List<Action<float>> noticList = notic[key];
                foreach (Action<float> action in noticList)
                {
                    if (action != null)
                    {
                        action(value);
                    }
                    else
                    {
                        noticList.Remove(action);
                    }
                }
            }
        }

        private void SaveLog(string key, float value)
        {
            if (!store.ContainsKey(key))
            {
                store.Add(key, value);
            }
            else
            {
                store[key] = store[key] + value;
            }
            Broadcast(key, value);
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