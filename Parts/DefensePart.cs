using UnityEngine;
using System;
using System.Collections;

namespace Gj
{
    public class DefensePart : BasePart
    {
        private Action<SkillInfo> skillNotic;
        private Action<ExtraInfo> extraNotic;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Effect(float value, GameObject obj)
        {
            float health = GetAttribute("health");
            health -= value;
            if (health <= 0)
            {
            }
            SetAttribute("health", health);
        }

        public void SetNotic(Action<SkillInfo> notic)
        {
            this.skillNotic = notic;
        }

        public void SetNotic(Action<ExtraInfo> notic)
        {
            this.extraNotic = notic;
        }

        public void BeCast(SkillInfo skillInfo)
        {
            if (skillNotic != null)
            {
                skillNotic(skillInfo);
            }
            Effect(skillInfo.value, skillInfo.master);
        }

        public void BeCast(ExtraInfo extraInfo)
        {
            if (extraNotic != null)
            {
                extraNotic(extraInfo);
            }
            Effect(extraInfo.value, extraInfo.master);
        }
    }
}