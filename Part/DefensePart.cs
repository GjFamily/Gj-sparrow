using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<float> notic;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetNotic(Action<float> notic) {
            this.notic = notic;
        }

        public void BeAttacked(float power) {
            if (notic != null) {
                notic(power);
            }
        }
    }
}