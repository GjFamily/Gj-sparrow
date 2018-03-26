using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BaseAi : MonoBehaviour
    {
        public virtual void Init() {
            GetComponent<Info>().ai = true;
        }
    }
}
