using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MovePart : BasePart
    {
        private GameObject target;
        private Vector3 end = Vector3.zero;
        private Vector3 direction = Vector3.zero;
        private float speed = 0;
        private bool moving = false;
        private bool needHit = false;
        private float hitDirection = 0;
        // Use this for initialization
        void Start()
        {

        }

        public void NeedHit(float direction)
        {
            needHit = true;
            hitDirection = direction;
        }

        public void NotNeedHit()
        {
            needHit = false;
        }

        public void SetTarget(GameObject target, float speed)
        {
            moving = true;
            this.target = target;
            this.speed = speed;
        }

        public void SetEnd(Vector3 end, float speed)
        {
            moving = true;
            this.end = end;
            this.speed = speed;
        }

        public void SetDirection(Vector3 direction, float speed)
        {
            moving = true;
            this.direction = direction;
            this.speed = speed;
        }

        public void Cancel()
        {
            this.target = null;
            this.direction = Vector3.zero;
            this.end = Vector3.zero;
            this.speed = 0;
            this.moving = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (moving && !IsHit())
            {
                if (target != null)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
                }
                else if (!Vector3.zero.Equals(end))
                {
                    if (transform.position.Equals(end))
                    {
                        Cancel();
                    }
                    else
                    {
                        transform.position = Vector3.MoveTowards(transform.position, end, Time.deltaTime * speed);
                    }
                }
                else if (!Vector3.zero.Equals(direction))
                {
                    transform.Translate(direction * Time.deltaTime * speed, Space.World);
                }
            }
        }

        bool IsHit()
        {
            if (!needHit) return false;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, hitDirection))
            {
                InfoPart info = hit.transform.gameObject.GetComponent<InfoPart>();
                if (info != null)
                {
                    if (info.IsBuild())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
