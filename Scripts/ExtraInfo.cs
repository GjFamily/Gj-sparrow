using UnityEngine;
using System.Collections;

namespace Gj
{
    public class ExtraInfo : MonoBehaviour
    {
        public string skillName;
        public float time;
        public float intervalTime;
        public RargetRelation targetRelation;
        public enum RargetRelation
        {
            Self,
            Partner,
            Enemy
        }
        public ExtraType extraType;
        public enum ExtraType
        {
            Damage,
            Attribute,
            Special
        }
        public float value;
        public TargetInfo.Type attributeType;
        public HandleType handleType;
        public enum HandleType
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public float HandleAttribute(float attribute)
        {
            switch (handleType)
            {
                case HandleType.Add:
                    return attribute + value;
                case HandleType.Subtract:
                    return attribute - value;
                case HandleType.Multiply:
                    return attribute * value;
                case HandleType.Divide:
                    return attribute / value;
                default:
                    return attribute;
            }
        }

        public float RecoveryAttribute(float attribute)
        {
            switch (handleType)
            {
                case HandleType.Add:
                    return attribute - value;
                case HandleType.Subtract:
                    return attribute + value;
                case HandleType.Multiply:
                    return attribute / value;
                case HandleType.Divide:
                    return attribute * value;
                default:
                    return attribute;
            }
        }
    }
}
