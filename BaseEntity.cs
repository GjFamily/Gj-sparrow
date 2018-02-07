using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    public class BaseEntity : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
        }
    }
}
