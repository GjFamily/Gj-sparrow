using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MonitorPart : BasePart
    {
        private GameObject target;
        private bool monitoring = false;
        private Action dieNotic;

        public void SetTarget (GameObject obj, Action action) {
            target = obj;
            monitoring = true;
            dieNotic = action;
        }
        // Update is called once per frame
        void Update()
        {
            if (monitoring) {
                if (target == null) {
                    if (dieNotic != null) {
                        dieNotic();
                        monitoring = false;
                    }
                }
            }
        }
    }
}
