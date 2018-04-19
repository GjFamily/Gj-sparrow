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
        [HideInInspector]
        public string showName;

        protected GameObject entity;

        protected void SetEntity(string entityName)
        {
            if (entity != null)
            {
                if (entity.name == entityName)
                {
                    return;
                }
                else
                {
                    ObjectService.single.DestroyObj(entity);
                }
            }
            else
            {
                entity = ObjectService.single.MakeObj(entityName, gameObject);
            }
        }

        protected T SetPlugin<T>(T t, string pluginName) where T : Component
        {
            if (t == null)
            {
                t = ObjectService.single.MakeObj(pluginName, gameObject).GetComponent<T>();
            }
            return t;
        }

        public virtual void Init()
        {
            Open();
        }

        protected void Open()
        {
            gameObject.SetActive(true);
            Info.live = true;
        }

        protected void Close()
        {
            gameObject.SetActive(false);
            Info.live = false;
            ObjectService.single.DestroyObj(gameObject);
        }

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
