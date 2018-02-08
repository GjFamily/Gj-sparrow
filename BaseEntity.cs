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

        public bool AllowAttack(GameObject obj)
        {
            return AllowAttack(gameObject, obj);
        }

        public bool AllowAttack(GameObject obj1, GameObject obj2)
        {
            RelationPart relationPart = obj1.GetComponent<RelationPart>();
            if (relationPart != null)
            {
                return relationPart.IsEnemy(obj2);
            }
            else
            {
                return false;
            }
        }
    }
}
