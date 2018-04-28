using UnityEngine;
using System.Collections;
using SimpleJSON;

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

        public virtual void FormatExtend (JSONObject json) {
            
        }

        public virtual void Init()
        {
            Open();
        }

        public void Init(TargetAttr attr, GameObject obj)
        {
            Info.attr = attr;
            Info.master = obj;
            FormatExtend(attr.extend);
            Init();
        }

        protected void Open()
        {
            Info.live = true;
        }

        protected void Close()
        {
            Info.live = false;
            ControlService.single.DestroyControl(gameObject);
        }

        protected virtual void Command(string type, string category, float value) { }
    }
}
