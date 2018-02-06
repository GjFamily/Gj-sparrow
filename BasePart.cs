using UnityEngine;
using System;

namespace Gj
{
    public class BasePart : BaseComponent
    {
        private GameObject _model;
        public GameObject Model {
            get {
                return _model;
            }
            set {
                _model = value;
                this.ModelBindPart();
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

        void ModelBindPart () {

            Type type = this.GetType();

            foreach (System.Object attributes in type.GetCustomAttributes(false))
            {
                ModelRequirPart modelRequirePart = (ModelRequirPart)attributes;
                if (null != modelRequirePart)
                {
                    if (_model.GetComponent(modelRequirePart.part) == null)
                    {
                        _model.AddComponent(modelRequirePart.part);
                    }
                }
            }
        }
    }
}
