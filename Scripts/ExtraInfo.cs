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
            Cast,
            Attribute,
            Special
        }
        public float value;
        public TargetInfo.Attr attribute;
        public HandleType handleType;
        public enum HandleType
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }
        private float startTime;
        private float lastTime;
        public GameObject master;

        public void Ready(){
            startTime = Time.time;
        }

        public bool Over(){
            return Time.time - startTime > time;
        }

        public bool NeedCast() {
            if (extraType == ExtraInfo.ExtraType.Cast) {
                float _t = Time.time - lastTime;
                if (_t > intervalTime) {
                    lastTime = _t;
                    return true;
                }
            }
            return false;
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
