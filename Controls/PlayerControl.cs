using UnityEngine;
using System.Collections;

namespace Gj
{
    public class PlayerControl : BaseControl
    {
        public virtual void LeftRockerEnter(string key) { }

        public virtual void LeftRocker(float angle, float h, float v) { }

        public virtual void LeftRockerExit(string key) { }

        public virtual void RightRockerEnter(string key) { }

        public virtual void RightRocker(float angle, float h, float v) { }

        public virtual void RightRockerExit(string key) { }

        public void Message(string type) {
            Message(type, "", 0);
        }
        public void Message(string type, string category) {
            Message(type, category, 0);
        }
        public virtual void Message(string type, string category, float value) { }
    }
}
