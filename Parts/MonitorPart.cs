using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class MonitorPart : BasePart
    {
        private GameObject target;
        private bool targeting = false;
        private List<GameObject> targetList = new List<GameObject>();
        private Action<GameObject> changeNotic;

        public void SetChangeTarget(Action<GameObject> action)
        {
            changeNotic = action;
        }

        public void AddTarget(GameObject obj)
        {
            if (target == null)
            {
                ChangeTarget(obj);
            }
            else
            {
                targetList.Add(obj);
            }
        }

        public void ChangeTarget(GameObject obj)
        {
            target = obj.GetComponent<GameObject>();
            targeting = true;
            if (changeNotic != null)
            {
                changeNotic(obj);
            }
        }

        private void NextTarget()
        {
            if (targetList.Count > 0)
            {
                GameObject obj = targetList[0];
                targetList.RemoveAt(0);
                if (obj != null)
                {
                    ChangeTarget(obj);
                }
                else
                {
                    NextTarget();
                }
            }
            else
            {
                targeting = false;
                changeNotic(null);
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (targeting)
            {
                if (CoreTools.GetInfo(target).live == false)
                {
                    NextTarget();
                }
            }
        }
    }
}
