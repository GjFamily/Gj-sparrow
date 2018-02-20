using UnityEngine;
using System;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
    [RequirePart(typeof(InfoPart))]
    public class BaseFeature : MonoBehaviour
    {
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
    }
}
