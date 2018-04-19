using UnityEngine;
using System.Collections;

namespace Gj
{
    public class FollowCamera : MonoBehaviour
    {
        public float speed = 5;
        GameObject target;
        Vector3 offsetPosition;
        float r;

        // Use this for initialization
        void Start()
        {
            r = transform.position.y / transform.position.z;
            offsetPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                transform.position = Vector3.Lerp(transform.position, target.transform.position + offsetPosition, Time.deltaTime * speed);
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
