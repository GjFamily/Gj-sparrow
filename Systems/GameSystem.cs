using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gj.Galaxy.Logic;

namespace Gj
{
    public class GameSystem : BaseSystem, GameListener, GameRoomListener
    {
        [SerializeField]
        private GameObject container;

        [SerializeField]
        private GameObject[] objs;


        protected override void Awake()
        {
            base.Awake();
            ObjectManage.single.SetObjs(objs);
            ObjectManage.single.SetContainer(container);
            StatisticsManage.single.Start();
        }

        protected TargetEntity MakeTarget(string targetName)
        {
            GameObject obj = ObjectManage.single.MakeObj(targetName);
            if (obj != null)
            {
                return obj.GetComponent<TargetEntity>();
            }
            else
            {
                return null;
            }
        }

        protected TargetEntity MakeTarget(string targetName, Vector3 position)
        {
            TargetEntity target = MakeTarget(targetName);
            if (target != null)
            {
                target.transform.position = position;
                return target;
            }
            else
            {
                return null;
            }
        }

        public void OnCommand(NetworkEntity entity, GamePlayer player, string type, string category, float value)
        {
            TargetEntity targetEntity = entity.GetComponent<TargetEntity>();
            if (targetEntity != null) targetEntity.Command(type, category, value, false);
        }

        public void OnDestroyInstance(GameObject gameObject, GamePlayer player)
        {
            Debug.Log("destroy");
            TargetEntity targetEntity = gameObject.GetComponent<TargetEntity>();
            if (targetEntity != null) targetEntity.Die(false);
        }

        public void OnFinish(bool exit, Dictionary<string, object> result)
        {
            Debug.Log("[ SOCKET ] Finish Game");
        }

        public GameObject OnInstance(string prefabName, GamePlayer player, object data)
        {
            TargetEntity targetEntity = MakeTarget(prefabName);
            targetEntity.Init(false);
            return targetEntity.gameObject;
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

        public void OnEnter()
        {
            Debug.Log("[ SOCKET ] Room Enter");
            GameConnect.ReadyGame();
        }

        public virtual void OnFail(string reason)
        {
            Debug.Log("[ SOCKET ] Game fail");
        }

        public virtual void OnRoomChange(Dictionary<string, object> props)
        {
            Debug.Log("[ SOCKET ] Room Change");
        }

        public virtual void OnPlayerJoin(GamePlayer player)
        {
            Debug.Log("[ SOCKET ] Player join");
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

