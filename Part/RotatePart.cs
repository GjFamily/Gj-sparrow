using UnityEngine;
using System.Collections;

namespace Gj
{
    public class RotatePart : BasePart
    {
        private GameObject target;
        private float angle = 0;
        private float speed = 0;
        private bool rotating = false;
        // Use this for initialization
        void Start()
        {

        }

        public void SetTarget (GameObject target) {
            this.target = target;
        }

        public void SetAngle(float angle)
        {
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        public void SetAngle(float angle, float speed)
        {
            this.angle = angle;
            this.speed = speed;
        }

        public void Cancel()
        {
            this.target = null;
            this.angle = 0;
            this.speed = 0;
            this.rotating = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null) {
                transform.LookAt(target.transform);
            } else if (speed > 0)
            {
                if (transform.rotation.eulerAngles.y == angle)
                {
                    Cancel();
                }
                else
                {
                    rotating = true;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), Time.deltaTime * speed);
                }
            }
        }
    }
}
