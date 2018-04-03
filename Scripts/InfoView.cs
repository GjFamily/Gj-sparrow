using System.Collections;
using System.Collections.Generic;
using System;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj
{
    [RequireComponent(typeof(Info))]
    public class InfoView : MonoBehaviour, GameObservable
    {
        public string[] infoList = null;
        public bool flag = false;
        Info info;

        void Awake()
        {
            this.info = GetComponent<Info>();
        }

        public void OnSerializeEntity(StreamBuffer stream, MessageInfo messageInfo)
        {
            foreach(var attr in infoList){
                stream.SendNext(info.GetAttribute(attr));
            }
        }

        public void OnDeserializeEntity(StreamBuffer stream, MessageInfo messageInfo)
        {
            foreach(var attr in infoList){
                info.SetAttribute(attr, stream.ReceiveNext<float>());
            }
        }

        public void BindEntity(NetworkEntity entity)
        {
        }
    }
}
