using System;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj{

    [RequireComponent(typeof(Info))]
    [RequireComponent(typeof(TransformView))]
    public class SyncSpeed : MonoBehaviour
    {
        Info info;
        TransformView tv;
        void Awake()
        {
            info = GetComponent<Info>() as Info;
            tv = GetComponent<TransformView>() as TransformView;
        }

        void Update()
        {
            tv.SetSynchronizedValues(info.GetAttribute("moveSpeed"), info.GetAttribute("rotateSpeed"));
        }
    }
}