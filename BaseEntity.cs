using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace Gj
{
    [RequirePart(typeof(Info))]
    public class BaseEntity : MonoBehaviour
    {
        private Info _info;
        public Info Info
        {
            get
            {
                if (_info == null) {
                    _info = GetComponent<Info>();
                }
                return _info;
            }
        }
        [HideInInspector]
        public bool show = false;

        private AllowSync sync = null;

        void Awake ()
        {
            CoreTools.BindPart(this, gameObject);
            CoreTools.AddFeature(this, gameObject);
            sync = CoreTools.AllowSync(this, gameObject);
            CoreTools.InfoSync(this, gameObject);
        }

        public void SyncInit(){
            if (sync != null) sync.Init();
        }

        public void SyncDestroy(){
            if (sync != null) sync.Destroy();
        }

        public void SyncCommand(string type, string category, float value)
        {
            if (sync != null) sync.Command(type, category, value);
        }

        public void Takeover(Action<bool> callback)
        {
            if (sync != null)
            {
                sync.Takeover(callback);
            }
            else
            {
                callback(true);
            }
        }

        public void Giveout()
        {
            if (sync != null) sync.Giveout();
        }

        public float GetAttribute (string key) {
            return Info.GetAttribute(key);
        }

        public void SetAttribute (string key, float value) {
            Info.SetAttribute(key, value);
        }

        protected virtual void Appear () {
            show = true;
            gameObject.SetActive(true);
        }

        protected virtual void Disappear () {
            show = false;
            gameObject.SetActive(false);
            CacheManage.single.SetCache(name, gameObject);
        }
    }
}
