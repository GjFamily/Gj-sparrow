using UnityEngine;
using System.Collections;
using System;

namespace Gj
{
    public class TargetInfo : MonoBehaviour
    {
        public float speed;
        public enum Attr {
            Speed
        }

        public void ChangeValue (Attr attr, Func<float, float> func) {
            switch(attr) {
                case Attr.Speed:
                    speed = func(speed);
                    break;
            }
        }
    }
}
