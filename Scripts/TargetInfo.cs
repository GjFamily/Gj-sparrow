using UnityEngine;
using System.Collections;

namespace Gj
{
    public class TargetInfo : MonoBehaviour
    {
        public float speed;
        public enum Type {
            Speed
        }

        public float GetValue (TargetInfo.Type type) {
            switch(type) {
                
            }
            return 0;
        }
    }
}
