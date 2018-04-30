using UnityEngine;
using System.Collections;

namespace Gj
{
    public class PlayerControl : BaseControl
    {
        public virtual void LeftRockerEnter(int key) { }

        public virtual void LeftRocker(float angle, float h, float v) { }

        public virtual void LeftRockerExit(int key) { }

        public virtual void RightRockerEnter(int key) { }

        public virtual void RightRocker(float angle, float h, float v) { }

        public virtual void RightRockerExit(int key) { }

        public virtual void ScreenRockerEnter() { }

        public virtual void ScreenRocker(float angle, float h, float v) { }

        public virtual void ScreenRockerExit() { }

        public void Message(int type) {
            Message(type, 0, 0);
        }
        public void Message(int type, int category) {
            Message(type, category, 0);
        }
        public virtual void Message(int type, int category, float value) { }
    }
}
