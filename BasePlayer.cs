using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BasePlayer : MonoBehaviour
    {
        protected virtual void Awake()
        {
            CoreTools.BindPart(this, gameObject);
        }
        public virtual void Init()
        {
            GetComponent<Info>().player = true;
        }

        public virtual void LeftRockerEnter(string key) { }

        public virtual void LeftRocker(float angle, float h, float v) { }

        public virtual void LeftRockerExit(string key) { }

        public virtual void RightRockerEnter(string key) { }

        public virtual void RightRocker(float angle, float h, float v) { }

        public virtual void RightRockerExit(string key) { }

        public virtual void Message(string key) { }
    }
}
