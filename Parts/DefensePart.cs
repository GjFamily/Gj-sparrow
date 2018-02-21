using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<SkillInfo, GameObject> skillNotic;
        private Action<ExtraInfo, GameObject> extraNotic;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetNotic(Action<SkillInfo, GameObject> notic) {
            this.skillNotic = notic;
        }

        public void SetNotic(Action<ExtraInfo, GameObject> notic)
        {
            this.extraNotic = notic;
        }

        public void BeAttacked(SkillInfo skillInfo, GameObject target) {
            if (skillNotic != null) {
                skillNotic(skillInfo, target);
            }
        }

        public void BeAttacked(ExtraInfo extraInfo, GameObject target)
        {
            if (extraNotic != null)
            {
                extraNotic(extraInfo, target);
            }
        }
    }
}