using UnityEngine;
using System;

namespace Gj
{
    public class BasePart : MonoBehaviour
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
