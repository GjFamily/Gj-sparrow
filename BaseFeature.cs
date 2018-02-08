using UnityEngine;
using System;

namespace Gj
{
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
                Init();
            }
        }

        private void Init (){
            BeLongPart beLong = GetFeatureComponent<BeLongPart>();
            if (beLong != null) {
                beLong.SetMaster(gameObject);
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
