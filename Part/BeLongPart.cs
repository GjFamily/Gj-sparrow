using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BeLongPart : BasePart
    {
        private GameObject master;
        private bool ignore = false;

        public void SetMaster(GameObject obj, bool ignore = false)
        {
            this.master = obj;
            this.ignore = ignore;
        }

        public GameObject GetMaster(bool notIgnore = false)
        {
            if (!notIgnore && ignore)
            {
                return gameObject;
            } else {
                return this.master;
            }
        }
    }
}