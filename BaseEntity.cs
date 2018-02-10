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

        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
        }

        public bool AllowAttack(GameObject obj)
        {
            RelationPart relation1 = GetMaster(obj).GetComponent<RelationPart>();
            RelationPart relation2 = GetMaster(obj).GetComponent<RelationPart>();
            if (relation1 != null && relation2 != null)
            {
                return relation1.IsEnemy(relation2);
            }
            else
            {
                return false;
            }
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
