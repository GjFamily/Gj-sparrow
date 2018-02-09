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

        public void SetLoseTargetNotci (Action<GameObject> action) {
            LoseTarget = action;
        }

        private void OnTriggerEnter(Collider other)
        {
            RelationPart relation = other.gameObject.GetComponent<RelationPart>();
            if (relation != null && relation.IsTarget() && FindTarget != null) {
                FindTarget(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            RelationPart relation = other.gameObject.GetComponent<RelationPart>();
            if (relation != null && relation.IsTarget() && FindTarget != null)
            {
                LoseTarget(other.gameObject);
            }
            
        }
    }
}
