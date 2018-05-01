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
            return attr.category != ObjectCategory.Object;
        }

        public bool IsTarget()
        {
            return attr.category == ObjectCategory.Target;
        }

        public bool IsPartner()
        {
            return attr.identity == ObjectIdentity.Partner;
        }

        public bool IsEnemy()
        {
            return attr.identity == ObjectIdentity.Enemy;
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
            if (attr.identity == ObjectIdentity.Enemy)
            {
                return info.attr.identity == ObjectIdentity.Enemy;
            }
            else if (attr.identity == ObjectIdentity.Partner)
            {
                return info.attr.identity == ObjectIdentity.Partner;
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
            if (attr.identity == ObjectIdentity.Enemy)
            {
                return info.attr.identity == ObjectIdentity.Partner;
            }
            else if (attr.identity == ObjectIdentity.Partner)
            {
                return info.attr.identity == ObjectIdentity.Enemy;
            }
            else
            {
                return false;
            }
        }
    }
}
