using UnityEngine;
using System.Collections.Generic;

namespace Gj
{
    public class BaseAi : MonoBehaviour
    {
        protected virtual void Awake()
        {
            CoreTools.BindPart(this, gameObject);
            CoreTools.BindFeature(this, gameObject);
            InitFeature();
        }
        public virtual void Init()
        {
            GetComponent<Info>().ai = true;
            Auto();
        }

        protected virtual void InitFeature()
        {

        }

        protected virtual void Auto()
        {

        }
    }
}
