using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Gj
{
    public class RelationService
    {
        public static RelationService single;

        private Dictionary<string, List<string>> sub;

        private Dictionary<string, string> store;

        static RelationService()
        {
            single = new RelationService();
        }

        public void Start()
        {
            store = new Dictionary<string, string>();
            sub = new Dictionary<string, List<string>>();
        }

        public void Register(string masterId, string id)
        {
            if (LoadMaster(id) == null)
            {
                SaveSub(masterId, id);
                SaveMaster(masterId, id);
            }
        }

        public string LoadMaster(string id)
        {
            if (store.ContainsKey(id))
            {
                return store[id];
            }
            return null;
        }

        private void SaveMaster(string masterId, string id)
        {
            store.Add(id, masterId);
        }

        public List<string> LoadSub(string masterId)
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
            if (LoadMaster(id) == null)
            {
                sub[masterId].Add(id);
            }
        }
    }
}