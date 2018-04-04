using System;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AllowSync : Attribute
    {
        //
        // Fields
        //
        public TransformParam transform = TransformParam.Off;
        public TransformOptions options;
        public RigidBodyParam rigid = RigidBodyParam.Off;

        public string[] infoList = null;
        public EntitySynchronization sync;

        private Component c;
        private GameObject o;

        //
        // Constructors
        //
        public AllowSync(EntitySynchronization sync)
        {
            this.sync = sync;
        }

        public void Register(Component c, GameObject o)
        {
            var entity = c.GetComponent<NetworkEntity>() as NetworkEntity;
            if (entity != null)
            {
                Debug.LogError("GameObject has NetworkEntity Component");
                return;
            }

            entity = o.AddComponent(typeof(NetworkEntity)) as NetworkEntity;
            entity.synchronization = sync;
            if (transform != TransformParam.Off)
            {
                var gameObservable = o.AddComponent(typeof(TransformView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as TransformView;
                if (ob != null)
                {
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
                if (ob != null)
                {
                    ob.rigidBodyParam = rigid;
                    ob.BindEntity(entity);
                }
            }
            if (infoList != null && infoList.Length > 0)
            {
                var gameObservable = o.AddComponent(typeof(InfoView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as InfoView;
                if (ob != null)
                {
                    ob.infoList = infoList;
                    ob.flag = true;
                    ob.BindEntity(entity);
                }
            }
            this.c = c;
            this.o = o;
        }

        public void Sync()
        {
            InstanceRelation relation;
            Info info = c.GetComponent<Info>();
            if (info == null && !info.player)
            {
                relation = InstanceRelation.Scene;
                Debug.Log("scene");
            }
            else
            {
                if (info.currentPlayer)
                {
                    relation = InstanceRelation.Player;
                    Debug.Log("player");
                }
                else
                {
                    relation = InstanceRelation.OtherPlayer;
                    Debug.Log("other player");
                }
            }
            GameConnect.RelationInstance(o.name, relation, o, 0, null);
        }

    }
}
