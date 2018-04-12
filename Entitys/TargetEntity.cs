using UnityEngine;
using System.Collections;

namespace Gj
{
    public class TargetEntity : BaseEntity
    {

        public virtual float Birth()
        {
            return 0;
        }

        public virtual float Attack()
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

        public virtual float Injured()
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

        public virtual float Die()
        {
            return 0;
        }
    }
}
