using UnityEngine;
using System.Collections;

namespace Gj
{
    public class CameraHelper : MonoBehaviour
    {
        public GameObject target;
        public float offsetX;
        public float offsetY;
        public float offsetZ;
        private Vector3 offsetPosition;

        // Use this for initialization
        void Start()
        {
            offsetPosition = new Vector3(offsetX, offsetY, offsetZ);
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                transform.position = offsetPosition + target.transform.position;
                transform.LookAt(target.transform);
            }
        }
    }
}
