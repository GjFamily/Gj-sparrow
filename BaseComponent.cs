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
            // 遍历 Rectangle 类的特性
            foreach (System.Object attributes in type.GetCustomAttributes(false))
            {
                AddPart addPart = (AddPart)attributes;
                if (null != addPart)
                {
                    gameObject.AddComponent(addPart.part);
                }
            }
        }
    }
}
