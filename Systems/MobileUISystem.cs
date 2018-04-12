using UnityEngine;
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

        private bool leftRockerTouch = false;
        private bool rightRochkerTouch = false;

        private string leftRockerKey = "";
        private string rightRockerKey = "";

        protected PlayerControl player;

        // Update is called once per frame
        protected virtual void Update()
        {
            if (leftRocker && (System.Math.Abs(SystemInput.lh) > 0 || System.Math.Abs(SystemInput.lv) > 0))
            {
                HandleRocker(SystemInput.lh, SystemInput.lv, true);
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
                    leftRockerKey = "";
                    leftRockerTouch = false;
                }
            }

            if (rightRocker && (System.Math.Abs(SystemInput.rh) > 0 || System.Math.Abs(SystemInput.rv) > 0))
            {
                HandleRocker(SystemInput.rh, SystemInput.rv, false);
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
                    rightRockerKey = "";
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

        protected virtual void LeftRockerExit(string key)
        {

            if (player != null)
            {
                player.LeftRockerExit(key);
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

        protected virtual void RightRockerExit(string key)
        {

            if (player != null)
            {
                player.RightRockerExit(key);
            }
        }
    }
}
