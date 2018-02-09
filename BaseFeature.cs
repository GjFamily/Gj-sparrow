using UnityEngine;
using System;

namespace Gj
{
    [RequirePart(typeof(BeLongPart))]
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
        protected bool ignoreMaster = false;

        protected virtual void Init (){
            GetFeatureComponent<BeLongPart>().SetMaster(gameObject, ignoreMaster);
        }

        protected GameObject GetMaster () {
            return GetFeatureComponent<BeLongPart>().GetMaster(true);
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
