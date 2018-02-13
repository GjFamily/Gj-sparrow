using UnityEngine;
using System.Collections;

namespace Gj
{
    public class FollowPart : BasePart
    {
        private float speed = 0;
        private GameObject target;
        private Vector3 offsetPosition;

        // Use this for initialization
        void Start()
        {
            offsetPosition = Vector3.zero - transform.position;
        }

        public void Cancel()
        {
            this.speed = 0;
            this.target = null;
        }

        public void FollowTarget(GameObject target, float speed)
        {
            this.speed = speed;
            this.target = target;
        }

        // Update is called once per frame
        void Update()
        {
            if (target && speed > 0)
            {
                transform.position = Vector3.Lerp(transform.position, target.transform.position - offsetPosition, Time.deltaTime * speed);
            }
        }
    }
}
