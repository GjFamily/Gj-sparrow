using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MobileGameSystem : BaseSystem
    {
        public bool leftRocker = false;
        public bool rightRocker = false;
        private bool leftRockerTouch = false;
        private bool rightRochkerTouch = false;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (leftRocker && (System.Math.Abs(SystemInput.lh) > 0 || System.Math.Abs(SystemInput.lv) > 0))
            {
                HandleRocker(SystemInput.lh, SystemInput.lv, true);
                if (!leftRockerTouch)
                {
                    LeftRockerEnter();
                    leftRockerTouch = true;
                }
            }
            else
            {
                if (leftRockerTouch)
                {
                    LeftRockerExit();
                    leftRockerTouch = false;
                }
            }

            if (rightRocker && (System.Math.Abs(SystemInput.rh) > 0 || System.Math.Abs(SystemInput.rv) > 0))
            {
                HandleRocker(SystemInput.rh, SystemInput.rv, false);
                if (!rightRochkerTouch)
                {
                    RightRockerEnter();
                    rightRochkerTouch = true;
                }
            }
            else
            {
                if (rightRochkerTouch)
                {
                    RightRockerExit();
                    rightRochkerTouch = false;
                }
            }
        }

        private void HandleRocker(float h, float v, bool left)
        {
            float angle = GetAngle(h, v);
            if (left)
            {
                LeftRocker(angle, h, v);
            }
            else
            {
                RightRocker(angle, h, v);
            }
        }

        private float GetAngle(float h, float v)
        {
            return Mathf.Atan2(h, v) * Mathf.Rad2Deg;
        }

        protected virtual void LeftRockerEnter() { }

        protected virtual void LeftRocker(float angle, float h, float v) { }

        protected virtual void LeftRockerExit() { }

        protected virtual void RightRockerEnter() { }

        protected virtual void RightRocker(float angle, float h, float v) { }

        protected virtual void RightRockerExit() { }
    }
}
