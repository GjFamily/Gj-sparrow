using UnityEngine;
using System.Collections;

namespace Gj
{
    public class Move : Part
    {
        private Vector3 end = Vector3.zero;
        private Vector3 direction = Vector3.zero;
        private float speed = 0;
        private bool moving = false;
        // Use this for initialization
        void Start()
        {

        }

        public void SetEnd(Vector3 end, float speed)
        {
            this.end = end;
            this.speed = speed;
        }

        public void SetDirection(Vector3 direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
        }

        public void Cancel()
        {
            this.direction = Vector3.zero;
            this.end = Vector3.zero;
            this.speed = 0;
            this.moving = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (speed > 0 && !Vector3.zero.Equals(end))
            {
                if (transform.position.Equals(end)) {
                    Cancel();
                } else {
                    moving = true;
                    transform.position = Vector3.Lerp(transform.position, end, Time.deltaTime * speed);
                }
            }
            else if (speed > 0 && !Vector3.zero.Equals(direction))
            {
                moving = true;
                transform.Translate(direction * Time.deltaTime * speed, Space.World);
            }
        }
    }
}
