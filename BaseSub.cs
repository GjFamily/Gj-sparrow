using UnityEngine;
using System;

namespace Gj
{
    public class BaseSub : MonoBehaviour
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
            }
        }

        protected T GetSubComponent<T>() {
            return Model.GetComponent<T>();
        }

        protected T[] GetSubComponents<T>()
        {
            return Model.GetComponents<T>();
        }
    }
}
