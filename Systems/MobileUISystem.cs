using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class MobileUISystem : MonoBehaviour
    {
        [SerializeField]
        private bool leftRocker = false;
        [SerializeField]
        private bool rightRocker = false;
        [SerializeField]
        private bool screenRocker = false;
        [SerializeField]
        private bool debug = false;

        private bool leftRockerTouch = false;
        private bool rightRochkerTouch = false;
        private bool screenRockerTouch = false;

        private int leftRockerKey = 0;
        private int rightRockerKey = 0;

        protected PlayerControl player;

        // Update is called once per frame
        protected virtual void Update()
        {
            if (debug)
            {
#if UNITY_EDITOR
                EditModel();
#else
            RealModel();
#endif
            }
            else
            {
                RealModel();
            }
        }

        private void EditModel()
        {
            float lv = !Input.GetKey("w") ? Input.GetKey("s") ? -1 : 0 : 1;
            float lh = !Input.GetKey("d") ? Input.GetKey("a") ? -1 : 0 : 1;
            if (System.Math.Abs(lh) > 0 || System.Math.Abs(lv) > 0)
            {
                HandleRocker(lh, lv, LeftRocker);
                if (!leftRockerTouch)
                {
                    leftRockerKey = 0;
                    LeftRockerEnter(leftRockerKey);
                    leftRockerTouch = true;
                }
            }
            else
            {
                if (leftRockerTouch)
                {
                    LeftRockerExit(leftRockerKey);
                    leftRockerKey = 0;
                    leftRockerTouch = false;
                }
            }

            float rv = !Input.GetKey("up") ? Input.GetKey("down") ? -1 : 0 : 1;
            float rh = !Input.GetKey("right") ? Input.GetKey("left") ? -1 : 0 : 1;
            if (System.Math.Abs(rh) > 0 || System.Math.Abs(rv) > 0)
            {
                HandleRocker(rh, rv, RightRocker);
                if (!rightRochkerTouch)
                {
                    rightRockerKey = 0;
                    RightRockerEnter(rightRockerKey);
                    rightRochkerTouch = true;
                }
            }
            else
            {
                if (rightRochkerTouch)
                {
                    RightRockerExit(rightRockerKey);
                    rightRockerKey = 0;
                    rightRochkerTouch = false;
                }
            }
        }

        private void RealModel()
        {
            if (leftRocker && (System.Math.Abs(SystemInput.lh) > 0 || System.Math.Abs(SystemInput.lv) > 0))
            {
                HandleRocker(SystemInput.lh, SystemInput.lv, LeftRocker);
                if (!leftRockerTouch)
                {
                    leftRockerKey = SystemInput.lk;
                    LeftRockerEnter(leftRockerKey);
                    leftRockerTouch = true;
                }
            }
            else
            {
                if (leftRockerTouch)
                {
                    LeftRockerExit(leftRockerKey);
                    leftRockerKey = 0;
                    leftRockerTouch = false;
                }
            }

            if (rightRocker && (System.Math.Abs(SystemInput.rh) > 0 || System.Math.Abs(SystemInput.rv) > 0))
            {
                HandleRocker(SystemInput.rh, SystemInput.rv, RightRocker);
                if (!rightRochkerTouch)
                {
                    rightRockerKey = SystemInput.rk;
                    RightRockerEnter(rightRockerKey);
                    rightRochkerTouch = true;
                }
            }
            else
            {
                if (rightRochkerTouch)
                {
                    RightRockerExit(rightRockerKey);
                    rightRockerKey = 0;
                    rightRochkerTouch = false;
                }
            }

            if (screenRocker && (System.Math.Abs(SystemInput.sh) > 0 || System.Math.Abs(SystemInput.sv) > 0))
            {
                HandleRocker(SystemInput.sh, SystemInput.sv, ScreenRocker);
                if (!screenRockerTouch)
                {
                    ScreenRockerEnter();
                    screenRockerTouch = true;
                }
            }
            else
            {
                if (screenRockerTouch)
                {
                    ScreenRockerExit();
                    screenRockerTouch = false;
                }
            }
        }

        private void HandleRocker(float h, float v, Action<float, float, float> action)
        {
            float angle = GetAngle(h, v);
            action(angle, h, v);
        }

        private float GetAngle(float h, float v)
        {
            return Mathf.Atan2(h, v) * Mathf.Rad2Deg;
        }

        protected virtual void LeftRockerEnter(int key)
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

        protected virtual void LeftRockerExit(int key)
        {

            if (player != null)
            {
                player.LeftRockerExit(key);
            }
        }

        protected virtual void RightRockerEnter(int key)
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

        protected virtual void RightRockerExit(int key)
        {

            if (player != null)
            {
                player.RightRockerExit(key);
            }
        }

        protected virtual void ScreenRockerEnter()
        {
            if (player != null)
            {
                player.ScreenRockerEnter();
            }
        }

        protected virtual void ScreenRocker(float angle, float h, float v)
        {

            if (player != null)
            {
                player.ScreenRocker(angle, h, v);
            }
        }

        protected virtual void ScreenRockerExit()
        {

            if (player != null)
            {
                player.ScreenRockerExit();
            }
        }
    }
}
