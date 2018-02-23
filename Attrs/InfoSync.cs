using System;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InfoSync : Attribute
    {
        //
        // Fields
        //
        public Type observable;
        public byte param = 0;

        //
        // Constructors
        //
        public InfoSync(Type observable) {
            this.observable = observable;
        }

        public void Register(Component c, GameObject o)
        {
            var entity = c.GetComponent<NetworkEntity>() as NetworkEntity;
            if (entity == null)
            {
                Debug.LogError("GameObject need NetworkEntity Component");
                return;
            }
            var gameObservable = o.AddComponent(observable);
            entity.ObservedComponents.Add(gameObservable);
            var ob = gameObservable as GameObservable;
            if (ob != null) ob.SetSyncParam(param);
        }
    }
}
