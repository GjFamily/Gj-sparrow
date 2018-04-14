using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gj.Galaxy.Logic;

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
            StatisticsService.single.Start();
        }

        protected T Make<T>(string targetName, Vector3 position) where T : Component
        {
            GameObject obj = ObjectService.single.MakeObj(targetName, container);
            if (obj != null)
            {
                obj.transform.position = position;
                return obj.AddComponent<T>();
            }
            else
            {
                return null;
            }
        }

        public void OnCommand(NetworkEntity entity, GamePlayer player, string type, string category, float value)
        {
            TargetEntity targetEntity = entity.GetComponent<TargetEntity>();
        }

        public void OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            Debug.Log("destroy");
            TargetEntity targetEntity = gameObject.GetComponent<TargetEntity>();
        }

        public void OnFinish(bool exit, Dictionary<string, object> result)
        {
            Debug.Log("[ SOCKET ] Finish Game");
        }

        public GameObject OnInstance(string prefabName, GamePlayer player, object data)
        {
            return null;
        }

        public void OnLeaveGame()
        {
            Debug.Log("[ SOCKET ] Leave Game");
        }

        public void OnOwnership(NetworkEntity entity, GamePlayer oldPlayer)
        {
            Debug.Log("[ SOCKET ] entity ownership change");
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
    }
}

