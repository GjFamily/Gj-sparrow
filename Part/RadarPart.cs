using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RadarPart : BasePart
    {
        private Action<GameObject> FindTarget;
        private Action<GameObject> LoseTarget;

        public void SetFindTargetNotic (Action<GameObject> action){
            FindTarget = action;
        }

        public void SetLoseTargetNotic (Action<GameObject> action) {
            LoseTarget = action;
        }

        private void OnTriggerEnter(Collider other)
        {
            InfoPart info = other.gameObject.GetComponent<InfoPart>();
            if (info != null && info.IsTarget() && FindTarget != null) {
                FindTarget(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            InfoPart info = other.gameObject.GetComponent<InfoPart>();
            if (info != null && info.IsTarget() && FindTarget != null)
            {
                LoseTarget(other.gameObject);
            }
            
        }
    }
}
