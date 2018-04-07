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
        protected virtual void Awake()
        {
            CoreTools.BindPart(this, gameObject);
        }

        protected float GetAttribute(string key)
        {
            if (Info != null) {
                return Info.GetAttribute(key);
            }
            return 0;
        }

        protected void SetAttribute(string key, float value)
        {
            if (Info != null) {
                Info.SetAttribute(key, value);
            }
        }
    }
}
