using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
    public class BaseComponent : MonoBehaviour
    {
        private void Awake()
        {
            BindPart();
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
                RequirePart requirePart = (RequirePart)attributes;
                if (null != requirePart)
                {
                    if (gameObject.GetComponent(requirePart.part) == null) {
                        gameObject.AddComponent(requirePart.part);
                    }
                }
            }
        }
    }
}
