using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    public class Info : MonoBehaviour
    {
        [HideInInspector]
        public bool live;
        [HideInInspector]
        public bool ai;
        [HideInInspector]
        public bool player;
        [HideInInspector]
        public bool otherPlayer;

        [HideInInspector]
        public GameObject master;

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

        private Identity identity = Identity.Empty;
        public enum Identity
        {
            Partner,
            Enemy,
            Empty
        }

        public void Init(JSONObject json)
        {

        }

        public object GetAll()
        {
            return "";
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

        public bool HaveBody() {
            return category != Category.Empty && category != Category.Skill;
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

        public void SetIdentity(Identity i)
        {
            identity = i;
        }

        public bool IsPartner()
        {
            return identity == Identity.Partner;
        }

        public bool IsEnemy()
        {
            return identity == Identity.Enemy;
        }

        public bool IsPartner(GameObject obj)
        {
            Info info = CoreTools.GetMasterInfo(obj);
            if (info != null)
            {
                return IsPartner(info);
            }
            else
            {
                return false;
            }
        }

        public bool IsPartner(Info info)
        {
            if (identity == Identity.Enemy)
            {
                return info.identity == Identity.Enemy;
            }
            else if (identity == Identity.Partner)
            {
                return info.identity == Identity.Partner;
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(GameObject obj)
        {
            Info info = CoreTools.GetMasterInfo(obj);
            if (info != null)
            {
                return IsEnemy(info);
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(Info info)
        {
            if (identity == Identity.Enemy)
            {
                return info.identity == Identity.Partner;
            }
            else if (identity == Identity.Partner)
            {
                return info.identity == Identity.Enemy;
            }
            else
            {
                return false;
            }
        }
    }
}
