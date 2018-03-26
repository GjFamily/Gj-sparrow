using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    public class Info : MonoBehaviour
    {
        public bool ai = false;
        public bool player = false;
        public bool currentPlayer = false;
        [Serializable]
        public struct Attribute
        {
            public string key;
            public float value;
        }
        public Attribute[] attrubutes;
        private Dictionary<string, float> attributeMap = new Dictionary<string, float>();
        private Category category = Category.Empty;
        public enum Category
        {
            Build,
            Empty,
            Skill,
            Target,
            BuildTarget,
            SkillTarget
        }

        void Awake()
        {
            if (attrubutes != null && attrubutes.Length > 0)
            {
                for (int i = 0; i < attrubutes.Length; i++)
                {
                    attributeMap.Add(attrubutes[i].key, attrubutes[i].value);
                }
            }
        }

        public void Init(JSONObject json)
        {

        }

        public float GetAttribute(string key)
        {
            return attributeMap.ContainsKey(key) ? attributeMap[key] : 0;
        }

        public void SetAttribute(string key, float value)
        {
            if (attributeMap.ContainsKey(key))
            {
                attributeMap[key] = value;
            }
            else
            {
                attributeMap.Add(key, value);
            }
        }

        public void SetCategory(Category c)
        {
            category = c;
        }

        public bool IsEmpty()
        {
            return category == Category.Empty;
        }

        public bool IsSkill()
        {
            return category == Category.Skill || category == Category.SkillTarget;
        }

        public bool IsTarget()
        {
            return category == Category.Target || category == Category.BuildTarget || category == Category.SkillTarget;
        }

        public bool IsBuild()
        {
            return category == Category.Build || category == Category.BuildTarget;
        }
    }
}
