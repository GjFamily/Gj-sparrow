using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    [RequirePart(typeof(RelationPart))]
    [RequirePart(typeof(Info))]
    public class BaseEntity : MonoBehaviour
    {
        protected Info Info
        {
            get
            {
                return GetComponent<Info>();
            }
        }
        [HideInInspector]
        public bool update = false;
        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
        }

        protected Info GetInfo (GameObject obj) {
            return obj.GetComponent<Info>();
        }

        protected float GetAttribute (GameObject obj, string key) {
            Info info = GetInfo(obj);
            if (info != null) {
                return info.GetAttribute(key);
            }
            return 0;
        }

        protected void SetAttribute(GameObject obj, string key, float value)
        {
            Info info = GetInfo(obj);
            if (info != null)
            {
                Info.SetAttribute(key, value);
            }
        }

        protected float GetAttribute (string key) {
            return Info.GetAttribute(key);
        }

        protected void SetAttribute (string key, float value) {
            Info.SetAttribute(key, value);
        }
    }
}
