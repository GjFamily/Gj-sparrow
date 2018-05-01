using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gj.Galaxy.Logic;
using System;

namespace Gj
{
    public class BaseGameSystem : MonoBehaviour, GameListener, GameReadyListener
    {
        [SerializeField]
        private GameObject container;

        [SerializeField]
        private GameObject[] objs;


        protected virtual void Awake()
        {
            ObjectService.single.SetObjs(objs);
            ObjectService.single.SetContainer(container);
            StatisticsService.single.Start();
        }

        public void OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            Debug.Log("destroy");
            TargetEntity targetEntity = gameObject.GetComponent<TargetEntity>();
        }

        public virtual void OnFinish(Dictionary<string, object> result)
        {
            Debug.Log("[ SOCKET ] Finish Game");
        }

        public void OnLeaveGame()
        {
            Debug.Log("[ SOCKET ] Leave Game");
        }

        public virtual void OnStart()
        {
            Debug.Log("[ SOCKET ] Game start");
        }

        public virtual void OnFail(string reason)
        {
            Debug.Log("[ SOCKET ] Game fail");
        }

        public virtual void OnPlayerLeave(GamePlayer player)
        {
            Debug.Log("[ SOCKET ] Player leave");
        }

        public virtual void OnPlayerRejoin(GamePlayer player)
        {
            Debug.Log("[ SOCKET ] Player Rejoin");
        }

        public virtual void OnPlayerChange(GamePlayer player, Dictionary<string, object> props)
        {
            Debug.Log("[ SOCKET ] Player Change");
        }

        public virtual void OnReadyPlayer(GamePlayer player)
        {
            Debug.Log("[ SOCKET ] Player Ready");
        }

        public virtual void OnReadyAll()
        {
            Debug.Log("[ SOCKET ] Player Ready");
        }

        public virtual void OnEnter(bool success)
        {
            throw new System.NotImplementedException();
        }

        public virtual GameObject OnInstance(string prefabName, InstanceRelation relation, GamePlayer player, Vector3 position, Quaternion rotation)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnSync(bool success)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnRequest(GamePlayer player, byte code, Dictionary<byte, object> value, Action<Dictionary<byte, object>> callback)
        {
            throw new NotImplementedException();
        }
    }
}

