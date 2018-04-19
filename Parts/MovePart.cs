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
        private bool targeting = false;
        private bool ending = false;
        private bool directing = false;
        private bool needHit = false;
        private bool auto = false;
        private UnityEngine.AI.NavMeshAgent agent = null;
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

        private void OpenAuto () {
            if (agent == null) {
                agent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            } else {
                agent.isStopped = false;
            }
            auto = true;
        }

        public void SetTarget(GameObject obj)
        {
            Cancel();
            moving = true;
            target = obj;
            targeting = true;
            if (GetAttribute("auto") > 0) {
                OpenAuto();
            }
        }

        public void SetEnd(Vector3 position)
        {
            Cancel();
            moving = true;
            end = position;
            ending = true;
            if (GetAttribute("auto") > 0)
            {
                OpenAuto();
            }
        }

        public void SetDirection(Vector3 d)
        {
            Cancel();
            moving = true;
            direction = d;
            directing = true;
        }

        public void Stop () {
            moving = false;
            if (auto) {
                agent.isStopped = true;
            }
        }

        public void Resume () {
            moving = true;
            if (auto)
            {
                agent.isStopped = false;
            }
        }

        public void Cancel()
        {
            target = null;
            direction = Vector3.zero;
            end = Vector3.zero;
            moving = false;
            directing = false;
            targeting = false;
            ending = false;
            if (auto) {
                agent.isStopped = true;
            }
            auto = false;
        }

        private void Move (Vector3 position, float speed) {
            if (auto) {
                agent.speed = speed;
                agent.SetDestination(position);
            } else {
                transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * speed);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (moving)
            {
                float speed = GetAttribute("moveSpeed");
                if (targeting)
                {
                    if (target != null) {
                        Move(target.transform.position, speed);
                    } else {
                        Cancel();
                    }
                }
                else if (ending)
                {
                    if (transform.position.Equals(end))
                    {
                        Cancel();
                    }
                    else
                    {
                        Move(end, speed);
                    }
                }
                else if (directing)
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
