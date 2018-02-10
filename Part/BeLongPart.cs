using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BeLongPart : BasePart
    {
        private GameObject master;

        public void SetMaster(GameObject obj)
        {
            master = obj;
        }

        public GameObject GetMaster()
        {
            return master;
        }
    }
}