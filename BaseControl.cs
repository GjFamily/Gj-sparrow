using UnityEngine;
using System.Collections;

namespace Gj
{
    public class BaseControl : MonoBehaviour
    {
        private Info _info;
        public Info Info
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
        public string showName;

        public float GetAttribute(string key)
        {
            return Info.GetAttribute(key);
        }

        public void SetAttribute(string key, float value)
        {
            Info.SetAttribute(key, value);
        }

        protected void Command (string type, string category, float value) {
            
        }
    }
}
