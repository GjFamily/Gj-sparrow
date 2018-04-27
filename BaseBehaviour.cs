using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BaseBehaviour : MonoBehaviour
    {
        private Info _info;
        protected Info Info
        {
            get
            {
                if (_info == null)
                {
                    _info = GetComponent<Info>();
                }
                return _info;
            }
        }
    }
}
