using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

namespace Gj
{
    public class Info : MonoBehaviour
    {
        [HideInInspector]
        public bool live;

        [HideInInspector]
        public GameObject master;
        [HideInInspector]
        public ObjectAttr attr;
        [HideInInspector]
        public ObjectControl control;

        public bool HaveBody() {
            return attr.baseAttr.category != ObjectCategory.Object;
        }

        public bool IsTarget()
        {
            return attr.baseAttr.category == ObjectCategory.Target;
        }

        public bool IsPartner()
        {
            return attr.baseAttr.identity == ObjectIdentity.Partner;
        }

        public bool IsEnemy()
        {
            return attr.baseAttr.identity == ObjectIdentity.Enemy;
        }

        public bool IsPartner(GameObject obj)
        {
            Info info = CoreTools.GetMasterInfo(obj);
            if (info != null)
            {
                return IsPartner(info);
            }
            else
            {
                return false;
            }
        }

        public bool IsPartner(Info info)
        {
            if (attr.baseAttr.identity == ObjectIdentity.Enemy)
            {
                return info.attr.baseAttr.identity == ObjectIdentity.Enemy;
            }
            else if (attr.baseAttr.identity == ObjectIdentity.Partner)
            {
                return info.attr.baseAttr.identity == ObjectIdentity.Partner;
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(GameObject obj)
        {
            Info info = CoreTools.GetMasterInfo(obj);
            if (info != null)
            {
                return IsEnemy(info);
            }
            else
            {
                return false;
            }
        }

        public bool IsEnemy(Info info)
        {
            if (attr.baseAttr.identity == ObjectIdentity.Enemy)
            {
                return info.attr.baseAttr.identity == ObjectIdentity.Partner;
            }
            else if (attr.baseAttr.identity == ObjectIdentity.Partner)
            {
                return info.attr.baseAttr.identity == ObjectIdentity.Enemy;
            }
            else
            {
                return false;
            }
        }
    }
}
