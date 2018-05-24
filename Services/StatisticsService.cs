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

        private Dictionary<string, ObjectAttr> store;

        static StatisticsService()
        {
            single = new StatisticsService();
        }

        public void Start()
        {
            store = new Dictionary<string, ObjectAttr>();
        }

        public void Register(string id, ObjectAttr attr)
        {
            if (LoadAttr(id) == null)
            {
                SaveAttr(id, attr);
            }
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

        public float Count(string id, StatisticsType type)
        {
            return Get(id, type) + SubCount(id, type);
        }

        private float SubCount(string masterId, StatisticsType type)
        {
            List<string> list = RelationService.single.LoadSub(masterId);
            if (list == null) return 0;
            float count = 0;
            foreach (string id in list)
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