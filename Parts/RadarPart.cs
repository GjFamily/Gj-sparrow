using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RadarPart : BasePart
    {
        private Action<GameObject> FindTarget;
        private Action<GameObject> LoseTarget;

        public void SetFindTargetNotic(Action<GameObject> action)
        {
            FindTarget = action;
        }

        public void SetLoseTargetNotic(Action<GameObject> action)
        {
            LoseTarget = action;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (FindTarget != null && CoreTools.IsTarget(other.gameObject))
            {
                FindTarget(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {

        }

        private void OnTriggerExit(Collider other)
        {
            if (LoseTarget != null && CoreTools.IsTarget(other.gameObject))
            {
                LoseTarget(other.gameObject);
            }

        }
    }
}
