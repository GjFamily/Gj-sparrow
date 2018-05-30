using UnityEngine;
using System.Collections;

namespace Gj
{
    public class PlayerControl : BaseControl
    {

        protected override void InitPlugin()
        {
            switch (Info.control)
            {
                case ObjectControl.Player:
                    InitPlayerPlugin();
                    break;
                case ObjectControl.OtherPlayer:
                    InitOtherPlayerPlugin();
                    break;
            }
        }

        protected virtual void InitPlayerPlugin() { }

        protected virtual void InitOtherPlayerPlugin() { }

        public virtual void LeftRockerEnter(int key) { }

        public virtual void LeftRocker(float angle, float h, float v) { }

        public virtual void LeftRockerExit(int key) { }

        public virtual void RightRockerEnter(int key) { }

        public virtual void RightRocker(float angle, float h, float v) { }

        public virtual void RightRockerExit(int key) { }

        public virtual void ScreenRockerEnter() { }

        public virtual void ScreenRocker(float angle, float h, float v) { }

        public virtual void ScreenRockerExit() { }
    }
}
