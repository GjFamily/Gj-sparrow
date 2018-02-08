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

        public bool IsPartner (RelationPart relation) {
            return true;
        }

        public bool IsEnemy (RelationPart relation) {
            return true;
        }


    }
}
