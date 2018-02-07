using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    public class BaseSystem : MonoBehaviour
    {
        [HideInInspector]
        public GameObject player;
        private bool leftRockerTouch = false;
        private bool rightRochkerTouch = false;

        protected virtual void Awake()
        {
            Tools.BindPart(this, gameObject);
            Tools.AddSub(this, gameObject);
        }

        // Use this for initialization
        protected virtual void Start()
        {

        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (System.Math.Abs(SystemInput.lh) > 0 || System.Math.Abs(SystemInput.lv) > 0)
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

            if (System.Math.Abs(SystemInput.rh) > 0 || System.Math.Abs(SystemInput.rv) > 0)
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
                    RightROckerExit();
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

        protected virtual void UIClick(string key) { }

        protected virtual void LeftRockerEnter() { }

        protected virtual void LeftRocker(float angle, float h, float v) { }

        protected virtual void LeftRockerExit() { }

        protected virtual void RightRockerEnter() { }

        protected virtual void RightRocker(float angle, float h, float v) { }

        protected virtual void RightROckerExit() { }
    }
}
