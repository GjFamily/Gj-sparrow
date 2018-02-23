using UnityEngine;
using System.Collections;

namespace Gj
{
    public class FollowPart : BasePart
    {
        private GameObject target;
        private Vector3 offsetPosition;

        // Use this for initialization
        void Start()
        {
            offsetPosition = Vector3.zero - transform.position;
        }

        public void Cancel()
        {
            target = null;
        }

        public void FollowTarget(GameObject obj)
        {
            target = obj;
        }

        // Update is called once per frame
        void Update()
        {
            float speed = GetAttribute("moveSpeed");
            if (target != null && speed > 0)
            {
                transform.position = Vector3.Lerp(transform.position, target.transform.position - offsetPosition, Time.deltaTime * speed);
            } else {
                transform.position = target.transform.position - offsetPosition;
            }
        }
    }
}
