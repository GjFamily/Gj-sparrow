using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    [RequirePart(typeof(RelationPart))]
    [RequirePart(typeof(InfoPart))]
    public class BaseEntity : MonoBehaviour
    {
        public bool update = false;
        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
        }
    }
}
