using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class MobileUISystem : BaseSystem
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
                    LeftRockerEnter(SystemInput.lk);
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
                    RightRockerEnter(SystemInput.rk);
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

        protected virtual void LeftRockerEnter(string key)
        {
            if (player != null)
            {
                player.LeftRockerEnter(key);
            }
        }

        protected virtual void LeftRocker(float angle, float h, float v)
        {

            if (player != null)
            {
                player.LeftRocker(angle, h, v);
            }
        }

        protected virtual void LeftRockerExit()
        {

            if (player != null)
            {
                player.LeftRockerExit();
            }
        }

        protected virtual void RightRockerEnter(string key)
        {

            if (player != null)
            {
                player.RightRockerEnter(key);
            }
        }

        protected virtual void RightRocker(float angle, float h, float v)
        {
            if (player != null)
            {
                player.RightRocker(angle, h, v);
            }
        }

        protected virtual void RightRockerExit()
        {

            if (player != null)
            {
                player.RightRockerExit();
            }
        }
    }
}
