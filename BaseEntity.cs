using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    public class BaseEntity : MonoBehaviour
    {
        private void Awake()
        {
            BindPart();
            AddSub();
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

            foreach (object attributes in type.GetCustomAttributes(typeof(RequirePart), false))
            {
                RequirePart requirePart = attributes as RequirePart;
                if (null != requirePart)
                {
                    if (gameObject.GetComponent(requirePart.part) == null)
                    {
                        gameObject.AddComponent(requirePart.part);
                    }
                }
            }
        }

        void AddSub()
        {
            foreach (FieldInfo fieldInfo in this.GetType().GetFields())
            {
                if (fieldInfo.FieldType == typeof(GameObject))
                {
                    foreach (object attributes in fieldInfo.GetCustomAttributes(typeof(AddSub), false))
                    {
                        AddSub addSub = attributes as AddSub;
                        if (null != addSub)
                        {
                            if (gameObject.GetComponent(addSub.sub) == null)
                            {
                                BaseSub baseSub = gameObject.AddComponent(addSub.sub) as BaseSub;
                                baseSub.Model = fieldInfo.GetValue(this) as GameObject;
                            }
                        }
                    }
                }
            }
        }
    }
}
