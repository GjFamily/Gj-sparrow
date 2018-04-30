using UnityEngine;
using System.Collections;
using SimpleJSON;
using Gj.Galaxy.Logic;
using System;
using Gj.Galaxy.Scripts;

namespace Gj
{
    [RequireComponent(typeof(Info))]
    [RequireComponent(typeof(NetworkEsse))]
    public class BaseControl : MonoBehaviour, EsseBehaviour
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
        private NetworkEsse _esse;
        public NetworkEsse Esse{
            get
            {
                if (_esse == null)
                {
                    _esse = GetComponent<NetworkEsse>();
                }
                return _esse;
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
            // 关联同步关系
            if (Esse != null)
                Esse.Relation("", InstanceRelation.Player);
        }

        protected const string CLOSE = "Close";

        protected void Close()
        {
            Info.live = false;
            if (Esse != null)
                Esse.Destroy();
            ControlService.single.DestroyControl(gameObject);
        }

<<<<<<< HEAD
        protected virtual void Command(byte type, byte category, float value) { }
=======
        protected virtual void Command(string type, string category, float value)
        {
            if (Esse != null)
                Esse.Command(type, category, value, () =>
                {

                });
        }

        public bool GetData(StreamBuffer stream)
        {
            return false;
        }

        public void UpdateData(StreamBuffer stream)
        {
            throw new NotImplementedException();
        }

        public void OnOwnership(GamePlayer oldPlayer, GamePlayer newPlayer)
        {
            throw new System.NotImplementedException();
        }

        public void OnCommand(GamePlayer player, object type, object category, object value)
        {
            throw new System.NotImplementedException();
        }

        public void InitSync(NetworkEsse esse)
        {
            esse.synchronization = Synchronization.Reliable;
            esse.ownershipTransfer = OwnershipOption.Request;
            var transformView = gameObject.AddComponent(typeof(TransformView)) as TransformView;
            esse.BindComponent(transformView);
            // 自定义同步信息
            transformView.transformParam = TransformParam.PositionAndRotation;
            // 速度同步
            transformView.options.positionParam = PositionParam.FixedSpeed;
            transformView.options.positionSpeed = 3f;
        }
>>>>>>> c05089e0cd83c76d6e7b2d388af9f9a94ec591bd
    }
}
