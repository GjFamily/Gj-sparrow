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
        private bool moving = false;
        private bool needHit = false;
        // Use this for initialization
        void Start()
        {

        }

        public void NeedHit()
        {
            needHit = true;
        }

        public void NotNeedHit()
        {
            needHit = false;
        }

        public void SetTarget(GameObject obj)
        {
            Cancel();
            moving = true;
            target = obj;
        }

        public void SetEnd(Vector3 position)
        {
            Cancel();
            moving = true;
            end = position;
        }

        public void SetDirection(Vector3 direction)
        {
            Cancel();
            moving = true;
            this.direction = direction;
        }

        public void Cancel()
        {
            this.target = null;
            this.direction = Vector3.zero;
            this.end = Vector3.zero;
            this.moving = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (moving && !IsHit())
            {
                float speed = GetAttribute("moveSpeed");
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

            if (Physics.Raycast(transform.position, transform.forward, out hit, GetAttribute("radio")))
            {
                Info info = CoreTools.GetInfo(hit.transform.gameObject);
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
