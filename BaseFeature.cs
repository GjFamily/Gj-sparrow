using UnityEngine;
using System;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    [RequirePart(typeof(Info))]
    public class BaseFeature : MonoBehaviour
    {
        protected Info Info
        {
            get
            {
                return GetComponent<Info>();
            }
        }
        private GameObject _model;
        public GameObject Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                Tools.BindPart(this, _model);
                GetFeatureComponent<BeLongPart>().SetMaster(gameObject);
            }
        }

        protected T GetFeatureComponent<T>() {
            return Model.GetComponent<T>();
        }

        protected T[] GetFeatureComponents<T>()
        {
            return Model.GetComponents<T>();
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
            return Info.GetAttribute(key);
        }

        protected void SetAttribute(string key, float value)
        {
            Info.SetAttribute(key, value);
        }
    }
}
