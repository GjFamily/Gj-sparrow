using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(RelationPart))]
    [RequirePart(typeof(AttackPart))]
    [RequirePart(typeof(DefensePart))]
    public class TargetEntity : BaseEntity
    {
        // Use this for initialization
        protected virtual void Start()
        {
            GetComponent<DefensePart>().SetNotic(Damaged);
        }

        protected virtual void Damaged(float power) { }
    }
}
