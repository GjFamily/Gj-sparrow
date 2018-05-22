using UnityEngine;
using System.Collections;

namespace Gj
{
    public class FollowCamera : MonoBehaviour
    {
        private float speed = 5;
        private float distance = 5;
        GameObject target;
        Vector3 offsetPosition;
        Vector3 tmp;
        Quaternion offsetRotation;
        float r;

        // Use this for initialization
        void Start()
        {
            r = transform.position.y / transform.position.z;
            offsetPosition = transform.position;
            offsetRotation = transform.rotation;
            tmp = Vector3.zero;
        }

        private void Update()
        {
            if (target != null) {
                transform.position = Vector3.Lerp(transform.position, target.transform.position + offsetPosition + tmp, Time.deltaTime * speed);
            }
        }

        public void SetRocker(float h, float v)
        {
            tmp = new Vector3(h * distance, 0, v * distance);
        }

        public void SetDistance(float d)
        {
            offsetPosition = new Vector3(0, d, d / r);
        }

        public void StartFollow(GameObject obj)
        {
            target = obj;
        }

        public void StopFollow()
        {
            target = null;
        }
    }
}
