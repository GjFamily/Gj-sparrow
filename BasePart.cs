using UnityEngine;
using System;

namespace Gj
{
    public class BasePart : MonoBehaviour
    {

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
    }
}
