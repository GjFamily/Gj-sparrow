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

        protected byte dataCount = 255;
        public bool IsOwner
        {
            get
            {
                return true;
            }
        }
        public byte DataLength
        {
            get
            {
                return dataCount;
            }
        }

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
                        BoxCollider boxCollider = CoreTools.GetComponentRequire<BoxCollider>(gameObject);
                        boxCollider.isTrigger = baseAttr.trigger;
                        boxCollider.size = new Vector3(baseAttr.sizeX, baseAttr.sizeY, baseAttr.sizeZ);
                        boxCollider.center = new Vector3(baseAttr.centerX, baseAttr.centerY, baseAttr.centerZ);
                        break;
                    case ObjectCollider.Sphere:
                        SphereCollider sphereCollider = CoreTools.GetComponentRequire<SphereCollider>(gameObject);
                        sphereCollider.isTrigger = baseAttr.trigger;
                        sphereCollider.radius = baseAttr.radius;
                        sphereCollider.center = new Vector3(baseAttr.centerX, baseAttr.centerY, baseAttr.centerZ);
                        break;
                }
            }

            if (baseAttr.rigidbody)
            {
                Rigidbody r = CoreTools.GetComponentRequire<Rigidbody>(gameObject);
                r.isKinematic = baseAttr.kinematic;
                r.useGravity = baseAttr.gravity;
                r.drag = Mathf.Infinity;
                r.angularDrag = Mathf.Infinity;
                r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionY;
            }
        }

        protected void Unaffected()
        {
            Rigidbody r = GetComponent<Rigidbody>();
            if (r != null)
            {
                r.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        protected virtual void InitPlugin() { }

        public virtual void OnMaster() { }

        public virtual void Init()
        {
            Open();
        }

        public void Init(ObjectAttr attr, ObjectControl control, GameObject obj)
        {
            Info.attr = attr;
            Info.master = obj;
            Info.control = control;
            SetEntity(attr.entity);
            Init();
            if (first)
            {
                InitBase(Info.attr.baseAttr);
                InitPlugin();
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
            if (Esse != null)
                Esse.Destroy();
            ControlService.single.DestroyControl(gameObject);
        }

        public virtual void OnUpdateData(object obj){}

        protected virtual void OnUpdateData(byte index, float value)
        {
            throw new System.NotImplementedException();
        }

        protected virtual float UpdateData(byte index, float value)
        {
            OnUpdateData(index, value);
            return value;
        }

        public virtual void OnUpdateData(byte index, object data)
        {
            OnUpdateData(index, (float)data);
        }

        public virtual void OnCommand(GamePlayer player, Dictionary<byte, object> data)
        {
            this.OnCommand((byte)data[0], (byte)data[1], (int)data[2]);
        }

        protected virtual void OnCommand(byte type, byte category, float value)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void Command(byte type, byte category, float value)
        {
            if (Esse != null)
            {
                var data = new Dictionary<byte, object>();
                data[0] = type;
                data[1] = category;
                data[2] = value;
                Esse.Command(data, () =>
                {
                    OnCommand(type, category, value);
                });
            }
            else
                OnCommand(type, category, value);
        }

        public virtual bool GetInfo(StreamBuffer stream)
        {
            return false;
        }

        public virtual void InitInfo(StreamBuffer stream, Vector3 position, Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public virtual void InitSync(NetworkEsse esse)
        {
            esse.serializeStatus = Synchronization.Fixed;
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

        public virtual void OnBelong(GameObject gameObject, NetworkEsse esse)
        {
            throw new NotImplementedException();
        }

        public virtual void OnAssign(GamePlayer player)
        {
            throw new NotImplementedException();
        }

        public virtual void OnOwnership(GamePlayer oldPlayer, GamePlayer newPlayer)
        {
            throw new NotImplementedException();
        }

        public void Message(byte type)
        {
            Message(type, 0, 0);
        }
        public void Message(byte type, byte category)
        {
            Message(type, category, 0);
        }
        public virtual void Message(byte type, byte category, float value) { }

    }
}
