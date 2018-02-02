using UnityEngine;
using System.Collections;

namespace Gj
{
    public class CameraHelper : MonoBehaviour
    {
        public GameObject target;
        private float speed = 5;
        private Vector3 offsetPosition;

        // Use this for initialization
        void Start()
        {
            offsetPosition = target.transform.position - transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                transform.position = Vector3.Lerp(transform.position, target.transform.position - offsetPosition, Time.deltaTime * speed);
                //transform.LookAt(target.transform);
            }
        }
    }
}
