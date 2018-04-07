using UnityEngine;
using System;
using System.Collections.Generic;

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
        private GameObject _feature;
        public GameObject Feature
        {
            get
            {
                return _feature;
            }
            set
            {
                _feature = value;
                CoreTools.BindPart(this, _feature);
                GetFeatureComponent<BeLongPart>().SetMaster(gameObject);
            }
        }

        protected T GetFeatureComponent<T>() {
            return Feature.GetComponent<T>();
        }

        protected T[] GetFeatureComponents<T>()
        {
            return Feature.GetComponents<T>();
        }

        protected T AddFeatureComponent<T>() where T : Component
        {
            return Feature.AddComponent<T>();
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

        public void Show()
        {
            Feature.SetActive(true);
        }

        public void Hide()
        {
            Feature.SetActive(false);
        }
    }
}
