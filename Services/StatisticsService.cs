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
            public Action<byte, float> cateNotic;
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

        public void OnChange(byte type, byte category, Action<float> action)
        {
            OnChange(GetKey(type, category), new Notic { notic = action });
        }

        public void OnChange(byte type, Action<byte, float> action)
        {
            OnChange(GetKey(type), new Notic { cateNotic = action });
        }

        private void OnChange(string key, Notic n)
        {
            if (!noticDic.ContainsKey(key))
            {
                noticDic.Add(key, new List<Notic>());
            }
            noticDic[key].Add(n);
        }

        public void Record(GameObject player, GameObject target, byte type, byte category, float value)
        {
            Debug.LogFormat("{0} -> {1}: {2} {3} {4}", player.name, target.name, type, category, value);
            SaveLog(type, category, value);
        }

        public void Event(GameObject player, byte type, byte category, float value)
        {
            Debug.LogFormat("{0}: {1} {2} {3}", player.name, type, category, value);
            SaveLog(type, category, value);
        }

        private void SaveLog(byte type, byte category, float value)
        {
            SaveLog(GetKey(type), value);
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

        private List<Notic> GetNoticList(string key)
        {
            if (noticDic.ContainsKey(key))
            {
                return noticDic[key];
            }
            else
            {
                return null;
            }
        }

        private void Broadcast(byte type, byte category, float value)
        {
            List<Notic> noticList = GetNoticList(GetKey(type));
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
            if (category != 0)
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

        public float LoadLog(byte type, byte category)
        {
            if (type != 0 && category != 0)
            {
                return LoadLog(GetKey(type, category));
            }
            else
            {
                return 0;
            }
        }

        public float LoadLog(byte type)
        {
            return LoadLog(GetKey(type));
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

        private string GetKey(byte type)
        {
            return type + "*";
        }

        private string GetKey(byte type, byte category)
        {
            return type + "-" + category;
        }
    }
}