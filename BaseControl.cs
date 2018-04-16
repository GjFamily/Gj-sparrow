using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequireComponent(typeof(Info))]
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
        protected GameObject entity;

        protected void SetEntity (string entityName) {
            entity = ObjectService.single.MakeObj(entityName, gameObject);
        }

        public virtual void Init() { }

        public float GetAttribute(string key)
        {
            return Info.GetAttribute(key);
        }

        public void SetAttribute(string key, float value)
        {
            Info.SetAttribute(key, value);
        }

        protected virtual void Command(string type, string category, float value) { }
    }
}
