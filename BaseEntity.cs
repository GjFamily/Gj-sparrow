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
            RelationPart relation1 = GetRelationPart(obj1);
            RelationPart relation2 = GetRelationPart(obj2);
            if (relation1 != null && relation2 != null)
            {
                return relation1.IsEnemy(relation2);
            }
            else
            {
                Debug.Log(obj1.name);
                Debug.Log(obj2.name);
                return false;
            }
        }

        public RelationPart GetRelationPart(GameObject obj)
        {
            return GetMaster(obj).GetComponent<RelationPart>();
        }

        public GameObject GetMaster(GameObject obj)
        {
            if (obj.GetComponent<BeLongPart>() != null)
            {
                return obj.GetComponent<BeLongPart>().GetMaster();
            } else {
                return obj;
            }
        }
    }
}
