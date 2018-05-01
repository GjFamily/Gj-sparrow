using UnityEngine;
using System.Collections;
using SimpleJSON;
using Gj.Galaxy.Logic;
using System;
using Gj.Galaxy.Scripts;
using System.Collections.Generic;

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
        public NetworkEsse Esse
        {
            get
            {
                if (_esse == null)
                {
                    _esse = GetComponent<NetworkEsse>();
                }
                return _esse != null && _esse.Id != "" ? _esse : null;
            }
        }

        private bool first = true;
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

        protected virtual void InitBase(BaseAttr baseAttr)
        {
            if (baseAttr.collider != ObjectCollider.Empty)
            {
                switch (baseAttr.collider)
                {
                    case ObjectCollider.Box:
                        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                        boxCollider.isTrigger = baseAttr.trigger;
                        boxCollider.size = new Vector3(baseAttr.sizeX, baseAttr.sizeY, baseAttr.sizeZ);
                        boxCollider.center = new Vector3(baseAttr.centerX, baseAttr.centerY, baseAttr.centerZ);
                        break;
                    case ObjectCollider.Sphere:
                        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                        sphereCollider.isTrigger = baseAttr.trigger;
                        sphereCollider.radius = baseAttr.radius;
                        sphereCollider.center = new Vector3(baseAttr.centerX, baseAttr.centerY, baseAttr.centerZ);
                        break;
                }
            }

            if (baseAttr.rigidbody)
            {
                Rigidbody body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = baseAttr.kinematic;
                body.useGravity = baseAttr.gravity;
            }
        }

        protected virtual void InitPlayerPlugin () {
            
        }

        protected virtual void InitOtherPlayerPlugin () {
            
        }

        protected virtual void InitAiPlugin () {
            
        }

        public virtual void FormatExtend(JSONObject json)
        {

        }

        public virtual void Init()
        {
            Open();
        }

        public void Init(ObjectAttr attr, ObjectControl control, GameObject obj, bool isLocal)
        {
            Info.attr = attr;
            Info.master = obj;
            Info.control = control;
            FormatExtend(attr.extend);
            SetEntity(attr.entity);
            // 关联同步关系, master不同步
            if (isLocal && obj == null) SyncRelation(attr, control);
            Init();
            if (first)
            {
                InitBase(Info.attr.baseAttr);
                switch (control)
                {
                    case ObjectControl.Player:
                        InitPlayerPlugin();
                        break;
                    case ObjectControl.OtherPlayer:
                        InitOtherPlayerPlugin();
                        break;
                    case ObjectControl.Ai:
                        Debug.Log("InitAiPlugin");
                        InitAiPlugin();
                        break;
                }
                first = false;
            }
        }

        protected void Open()
        {
            Info.live = true;
        }

        protected const string CLOSE = "Close";

        protected void Close()
        {
            Info.live = false;
            if (Esse != null)
                Esse.Destroy();
            ControlService.single.DestroyControl(gameObject);
        }

        protected virtual void OnCommand(byte type, byte category, float value)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void OnOwnership(BaseControl baseControl)
        {
            
        }

        protected virtual void SyncRelation(ObjectAttr attr, ObjectControl control)
        {
            if (Esse == null) return;
            switch (control)
            {
                case ObjectControl.Player:
                    Esse.Relation(attr.name, InstanceRelation.Player);
                    break;
                case ObjectControl.OtherPlayer:
                    Esse.Relation(attr.name, InstanceRelation.OtherPlayer);
                    break;
                case ObjectControl.Ai:
                    Esse.Relation(attr.name, InstanceRelation.Scene);
                    break;
            }
        }

        protected virtual void Command(byte type, byte category, float value)
        {
            if (Esse != null)
                Esse.Command(type, category, value, () =>
                {
                    OnCommand(type, category, value);
                });
            else
                OnCommand(type, category, value);
        }

        public virtual bool GetData(StreamBuffer stream)
        {
            return false;
        }

        public virtual void UpdateData(StreamBuffer stream)
        {
            throw new NotImplementedException();
        }

        public virtual void OnOwnership(GamePlayer oldPlayer, GamePlayer newPlayer)
        {
            
        }

        public virtual void OnCommand(GamePlayer player, object type, object category, object value)
        {
            this.OnCommand((byte)type, (byte)category, (float)value);
        }

        public virtual void InitSync(NetworkEsse esse)
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
            transformView.options.extrapolatedParam = ExtrapolatedParam.FixedSpeed;
            transformView.options.extrapolatedPositionSpeed = 3f;
            transformView.options.rotationParam = RotationParam.FixedSpeed;
            transformView.options.rotationSpeed = 180;
        }

        public virtual void OnSurvey(Dictionary<byte, object> data)
        {
            throw new NotImplementedException();
        }
    }
}
