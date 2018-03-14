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
        public RigidBodyParam rigid = RigidBodyParam.Off;
        public EntitySynchronization sync;

        private Component c;
        private GameObject o;

        //
        // Constructors
        //
        public AllowSync(EntitySynchronization sync) {
            this.sync = sync;
        }

        public void Register(Component c, GameObject o){
            var entity = c.GetComponent<NetworkEntity>() as NetworkEntity;
            if (entity != null)
            {
                Debug.LogError("GameObject has NetworkEntity Component");
                return;
            }

            entity = o.AddComponent(typeof(NetworkEntity)) as NetworkEntity;
            entity.synchronization = sync;
            if(transform != TransformParam.Off){
                var gameObservable = o.AddComponent(typeof(TransformView));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as GameObservable;
                if (ob != null) ob.SetSyncParam((byte)transform);
            }
            if(rigid != RigidBodyParam.Off){
                var gameObservable = o.AddComponent(typeof(Rigidbody));
                entity.ObservedComponents.Add(gameObservable);
                var ob = gameObservable as GameObservable;
                if (ob != null) ob.SetSyncParam((byte)transform);
            }
            this.c = c;
            this.o = o;
        }

        public void Sync(){
            InstanceRelation relation;
            var player = c.GetComponent<PlayerEntity>() as PlayerEntity;
            if (player == null)
            {
                relation = InstanceRelation.Scene;
                Debug.Log("scene");
            }
            else
            {
                if (player.player)
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
