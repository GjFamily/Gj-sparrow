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
        Quaternion offsetRotation;
        float r;

        // Use this for initialization
        void Start()
        {
            r = transform.position.y / transform.position.z;
            offsetPosition = transform.position;
            offsetRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                Vector3 tmp = new Vector3(SystemInput.sh * distance, 0, SystemInput.sv * distance);
                Debug.Log(distance);
                Debug.Log(tmp);
                transform.position = Vector3.Lerp(transform.position, target.transform.position + offsetPosition + tmp, Time.deltaTime * speed);

            }
        }

        public void SetDistance (float d) {
            offsetPosition = new Vector3(0, d, d / r);
        }

        public void StartFollow(GameObject obj) {
            target = obj;
        }

        public void StopFollow()
        {
            target = null;
        }
    }
}
