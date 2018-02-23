using UnityEngine;
using System;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    [RequirePart(typeof(Info))]
    public class BaseFeature : MonoBehaviour
    {
        private Info _info;
        protected Info Info
        {
            get
            {
                if (_info == null)
                {
                    _info = GetComponent<Info>();
                }
                return _info;
            }
        }
        private Info _featureInfo;
        protected Info FeatureInfo
        {
            get
            {
                if (_featureInfo == null)
                {
                    _featureInfo = GetFeatureComponent<Info>();
                }
                return _featureInfo;
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

        protected float GetFeatureAttribute(string key)
        {
            return FeatureInfo.GetAttribute(key);
        }

        protected void SetFeatureAttribute(string key, float value)
        {
            FeatureInfo.SetAttribute(key, value);
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
