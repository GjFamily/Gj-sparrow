using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<SkillInfoPart, GameObject> notic;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetNotic(Action<SkillInfoPart, GameObject> notic) {
            this.notic = notic;
        }

        public void BeAttacked(SkillInfoPart skillInfo, GameObject target) {
            if (notic != null) {
                notic(skillInfo, target);
            }
        }
    }
}