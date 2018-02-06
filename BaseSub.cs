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
                this.BindPart();
            }
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void BindPart()
        {
            Type type = this.GetType();

            foreach (System.Object attributes in type.GetCustomAttributes(false))
            {
                SubRequirePart subRequirePart = (SubRequirePart)attributes;
                if (null != subRequirePart)
                {
                    if (_model.GetComponent(subRequirePart.part) == null)
                    {
                        _model.AddComponent(subRequirePart.part);
                    }
                }
            }
        }
    }
}
