using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RotatePart : BasePart
    {
        private GameObject target;
        private Vector3? targetPosition;
        private float angle = 0;
        private bool around = false;
        private bool rotating = false;
        // Use this for initialization
        void Start()
        {

        }

        public void TurnAround()
        {
            Cancel();
            rotating = true;
            around = true;
        }

        public void SetTarget(Vector3 position)
        {
            Cancel();
            targetPosition = position;
        }

        public void SetTarget(GameObject obj)
        {
            Cancel();
            target = obj;
        }

        public void SetAngle(float angle)
        {
            Cancel();
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        public void SetRotateAngle(float angle)
        {
            Cancel();
            rotating = true;
            this.angle = angle;
        }

        public void Cancel()
        {
            target = null;
            targetPosition = null;
            angle = 0;
            rotating = false;
            around = false;
        }

        // Update is called once per frame
        void Update()
        {
            float speed = Info.attr.rotate;
            if (target != null)
            {
                transform.LookAt(target.transform.position);
            }
            else if (targetPosition != null)
            {
                transform.LookAt(targetPosition.Value);
            }
            else if (around)
            {
                transform.RotateAround(transform.position, transform.up, speed);
            }
            else if (speed > 0 && rotating)
            {
                if (transform.rotation.eulerAngles.y == angle)
                {
                    Cancel();
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * speed);
                }
            }
        }
    }
}
