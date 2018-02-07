using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    public class BaseSystem : MonoBehaviour
    {
        protected GameObject player;
        protected virtual void Awake()
        {
            Tools.AddSub(this, gameObject);
        }

        // Use this for initialization
        protected virtual void Start()
        {

        }

        // Update is called once per frame
        protected virtual void Update()
        {
        }
    }
}
