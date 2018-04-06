using UnityEngine;
using System.Collections;

namespace Gj
{
    public class FollowPart : BasePart
    {
        private GameObject target;
        private Vector3 offsetPosition = Vector3.zero;

        // Use this for initialization
        void Start()
        {
        }

        public void Cancel()
        {
            target = null;
        }

        public void SetOffset (Vector3 offset) {
            offsetPosition = offset;
        }

        public void FollowTarget(GameObject obj, Vector3 offset)
        {
            target = obj;
            SetOffset(offset);
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
                transform.position = Vector3.Lerp(transform.position, target.transform.position + offsetPosition, Time.deltaTime * speed);
            } else if (target != null) {
                transform.position = target.transform.position + offsetPosition;
            }
        }
    }
}
