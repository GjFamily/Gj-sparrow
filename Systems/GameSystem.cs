using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class GameSystem : BaseSystem
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

    }
}

