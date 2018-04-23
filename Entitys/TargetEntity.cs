using UnityEngine;
using System.Collections;

namespace Gj
{
    public class TargetEntity : BaseEntity
    {
        public virtual void Init()
        {
            Idle();
        }

        public virtual float Idle()
        {
            return 0;
        }

        public virtual float Run()
        {
            return 0;
        }

        public virtual float Walk()
        {
            return 0;
        }

        public virtual float Talk()
        {
            return 0;
        }

        public virtual float Laugh()
        {
            return 0;
        }

        public virtual float Dizzy()
        {
            return 0;
        }

        public virtual float Escape()
        {
            return 0;
        }

        public virtual float Sleep()
        {
            return 0;
        }

        public virtual float GetUp()
        {
            return 0;
        }

        public virtual float Jump()
        {
            return 0;
        }

        public virtual float Attack()
        {
            return 0;
        }

        public virtual float AttackRepeat()
        {
            return 0;
        }

        public virtual float Cast()
        {
            return 0;
        }

        public virtual float Charge()
        {
            return 0;
        }

        public virtual float Defense()
        {
            return 0;
        }

        public virtual float Hit()
        {
            return 0;
        }

        public virtual float Die()
        {
            return 0;
        }
    }
}
