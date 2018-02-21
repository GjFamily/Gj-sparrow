using UnityEngine;
using System.Collections.Generic;

namespace Gj
{
    public class CameraManage
    {
        public Camera[] cameraList;
        private Dictionary<string, Camera> cameraMap = new Dictionary<string, Camera>();
        // Use this for initialization
        void Start()
        {
            foreach (Camera _camera in cameraList)
            {
                cameraMap.Add(_camera.name, _camera);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeCamera (string name) {
            foreach (var item in cameraMap)
            {
                if (item.Key == name) {
                    item.Value.gameObject.SetActive(true);
                } else {
                    item.Value.gameObject.SetActive(false);
                }
            }
        }
    }
}
