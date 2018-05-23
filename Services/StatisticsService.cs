using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class StatisticsService
    {
        public static StatisticsService single;

        public enum StatisticsType
        {
            KILL = 0
        }

        private Dictionary<string, List<string>> sub;

        private Dictionary<string, ObjectAttr> store;

        static StatisticsService()
        {
            single = new StatisticsService();
        }

        public void Start()
        {
            store = new Dictionary<string, ObjectAttr>();
            sub = new Dictionary<string, List<string>>();
        }

        public void Register(string id, ObjectAttr attr)
        {
            if (LoadAttr(id) == null)
            {
                SaveAttr(id, attr);
            }
        }

        public void Register(string masterId, string id, ObjectAttr attr)
        {
            Register(masterId, attr);
            SaveSub(masterId, id);
            Register(id, attr);
        }

        private ObjectAttr LoadAttr(string id)
        {
            if (store.ContainsKey(id))
            {
                return store[id];
            }
            return null;
        }

        private void SaveAttr(string id, ObjectAttr attr)
        {
            store.Add(id, attr);
        }

        private List<string> LoadSub(string masterId)
        {
            if (sub.ContainsKey(masterId))
            {
                return sub[masterId];
            }
            return null;
        }

        private void SaveSub(string masterId, string id)
        {
            if (LoadSub(masterId) == null)
            {
                sub.Add(masterId, new List<string>());
            }
            if (LoadAttr(id) == null)
            {
                sub[masterId].Add(id);
            }
        }

        public float Count(string id, StatisticsType type)
        {
            return Get(id, type) + SubCount(id, type);
        }

        private float SubCount(string masterId, StatisticsType type)
        {
            List<string> list = LoadSub(masterId);
            if (list == null) return 0;
            float count = 0;
            foreach (string id in sub[masterId])
            {
                count += Get(id, type);
            }
            return count;
        }

        private float Get(string id, StatisticsType type)
        {
            ObjectAttr attr = LoadAttr(id);
            if (attr == null) return 0;
            switch (type)
            {
                case StatisticsType.KILL:
                    return attr.kill;
            }
            return 0;
        }
    }
}