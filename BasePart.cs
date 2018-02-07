using UnityEngine;
using System;

namespace Gj
{
    public class BasePart : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
        }
    }
}
