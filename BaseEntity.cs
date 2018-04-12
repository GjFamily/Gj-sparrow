using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    public class BaseEntity : MonoBehaviour
    {
        [HideInInspector]
        public bool show = false;

        protected virtual void Appear () {
            show = true;
            gameObject.SetActive(true);
        }

        protected virtual void Disappear () {
            show = false;
            gameObject.SetActive(false);
            CacheService.single.SetCache(name, gameObject);
        }
    }
}
