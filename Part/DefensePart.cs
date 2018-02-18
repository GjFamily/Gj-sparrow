using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<float, GameObject> notic;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetNotic(Action<float, GameObject> notic) {
            this.notic = notic;
        }

        public void BeAttacked(float power, GameObject target) {
            if (notic != null) {
                notic(power, target);
            }
        }
    }
}