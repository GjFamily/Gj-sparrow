using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RelationPart : BasePart
    {
        private Identity identity;
        public enum Identity {
            Partner,
            Monster,
            Player,
            Empty
        }

        public void SetIdentity (Identity i) {
            identity = i;
        }

        public Identity GetIdentity () {
            return identity;
        }

        public bool IsPartner (GameObject obj) {
            return false;
        }

        public bool IsEnemy (GameObject obj) {
            return false;
        }

        private Identity GetIdentity (GameObject obj) {
            RelationPart reationPart = obj.GetComponent<RelationPart>();
            if (reationPart == null) {
                return Identity.Empty;
            } else {
                return reationPart.GetIdentity();
            }
        }


    }
}
