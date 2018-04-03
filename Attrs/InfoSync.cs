using System;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InfoSync : Attribute
    {
        public TransformParam transform = TransformParam.Off;
        public TransformOptions options;
        public RigidBodyParam rigid = RigidBodyParam.Off;

        public string[] infoList = null;

        private Component c;
        private GameObject o;

        //
        // Constructors
        //
        public InfoSync() {
            
        }

        public void Register(Component c, GameObject parent, GameObject o)
        {
            var entity = c.GetComponent<NetworkEntity>() as NetworkEntity;
            if (entity == null)
            {
                Debug.LogError("GameObject not has NetworkEntity Component");
                return;
            }

            if (transform != TransformParam.Off)
            {
                var gameObservable = o.AddComponent(typeof(TransformView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as TransformView;
                if (ob != null){
                    ob.transformParam = transform;
                    if (options != null) ob.options = options;
                    o.AddComponent(typeof(SyncSpeed));
                    ob.BindEntity(entity);
                }
            }
            if (rigid != RigidBodyParam.Off)
            {
                var gameObservable = o.AddComponent(typeof(RigidbodyView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as RigidbodyView;
                if (ob != null){
                    ob.rigidBodyParam = rigid;
                    ob.BindEntity(entity);
                }
            }
            if (infoList != null && infoList.Length > 0)
            {
                var gameObservable = o.AddComponent(typeof(InfoView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as InfoView;
                if (ob != null){
                    ob.infoList = infoList;
                    ob.flag = true;
                    ob.BindEntity(entity);
                }
            }
            this.c = c;
            this.o = o;
        }
    }

}
