using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    [RequirePart(typeof(Info))]
    public class BaseEntity : MonoBehaviour
    {
        private Info _info;
        public Info Info
        {
            get
            {
                if (_info == null) {
                    _info = GetComponent<Info>();
                }
                return _info;
            }
        }
        [HideInInspector]
        public bool show = false;

        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
            Tools.AllowSync(this, gameObject);
            Tools.InfoSync(this, gameObject);
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

        protected virtual void Appear () {
            show = true;
            gameObject.SetActive(true);
        }

        protected virtual void Disappear () {
            show = false;
            gameObject.SetActive(false);
            CacheManage.single.SetCache(name, gameObject);
        }
    }
}
