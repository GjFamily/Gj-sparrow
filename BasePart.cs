using UnityEngine;
using System;

namespace Gj
{
    [RequirePart(typeof(Info))]
    public class BasePart : MonoBehaviour
    {
        protected Info Info {
            get {
                return GetComponent<Info>();
            }
        }
        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
        }

        protected Info GetInfo(GameObject obj)
        {
            return obj.GetComponent<Info>();
        }

        protected float GetAttribute(GameObject obj, string key)
        {
            Info info = GetInfo(obj);
            if (info != null)
            {
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

        protected float GetAttribute(string key)
        {
            if (Info != null) {
                return Info.GetAttribute(key);
            }
            return 0;
        }

        protected void SetAttribute(string key, float value)
        {
            if (Info != null) {
                Info.SetAttribute(key, value);
            }
        }
    }
}
