using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RotatePart : BasePart
    {
        private GameObject target;
        private Vector3 targetPosition;
        private float angle = 0;
        private float speed = 0;
        private bool around = false;
        private bool rotating = false;
        // Use this for initialization
        void Start()
        {

        }

        public void TurnAround(float speed)
        {
            Cancel();
            this.speed = speed;
            this.rotating = true;
            this.around = true;
        }

        public void SetTarget(Vector3 target)
        {
            Cancel();
            this.targetPosition = target;
        }

        public void SetTarget(GameObject target)
        {
            Cancel();
            this.target = target;
        }

        public void SetAngle(float angle)
        {
            Cancel();
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        public void SetAngle(float angle, float speed)
        {
            Cancel();
            rotating = true;
            this.angle = angle;
            this.speed = speed;
        }

        public void Cancel()
        {
            this.target = null;
            this.angle = 0;
            this.speed = 0;
            this.rotating = false;
            this.around = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                transform.LookAt(target.transform.position);
            }
            else if (!targetPosition.Equals(Vector3.zero))
            {
                transform.LookAt(targetPosition);
            }
            else if (around)
            {
                transform.RotateAround(transform.position, transform.up, speed);
            }
            else if (speed > 0)
            {
                if (transform.rotation.eulerAngles.y == angle)
                {
                    Cancel();
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * speed);
                }
            }
        }
    }
}
