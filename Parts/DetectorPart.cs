using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class DetectorPart : BasePart
    {
        public GameObject FindNearEnemy()
        {
            List<GameObject> targetEntities = FindTarget();
            GameObject enemy = null;
            float distance = 0;
            float _distance = 0;
            foreach (GameObject t in targetEntities)
            {
                if (Info.IsEnemy(t.gameObject))
                {
                    if (enemy == null)
                    {
                        enemy = t;
                        distance = Vector3.Distance(transform.position, enemy.transform.position);
                    }
                    else
                    {
                        _distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (_distance < distance)
                        {
                            enemy = t;
                            distance = _distance;
                        }
                    }
                }
            }
            return enemy;
        }

        public List<GameObject> FindEnemy()
        {
            List<GameObject> targetEntities = FindTarget();
            List<GameObject> enemys = new List<GameObject>();

            foreach (GameObject t in targetEntities)
            {
                if (Info.IsEnemy(t.gameObject))
                {
                    enemys.Add(t);
                }
            }
            return enemys;
        }

        public GameObject FindNearPartner()
        {
            List<GameObject> targetEntities = FindTarget();
            GameObject enemy = null;
            float distance = 0;
            float _distance = 0;
            foreach (GameObject t in targetEntities)
            {
                if (Info.IsPartner(t.gameObject))
                {
                    if (enemy == null)
                    {
                        enemy = t;
                        distance = Vector3.Distance(transform.position, enemy.transform.position);
                    }
                    else
                    {
                        _distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (_distance < distance)
                        {
                            enemy = t;
                            distance = _distance;
                        }
                    }
                }
            }
            return enemy;
        }

        public List<GameObject> FindPartner()
        {
            List<GameObject> targetEntities = FindTarget();
            List<GameObject> partners = new List<GameObject>();

            foreach (GameObject t in targetEntities)
            {
                if (Info.IsPartner(t.gameObject))
                {
                    partners.Add(t);
                }
            }
            return partners;
        }

        public List<GameObject> FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Info.attr.scanRadius);
            List<GameObject> targetEntities = new List<GameObject>();
            foreach (Collider c in colliders)
            {
                if (CoreTools.IsTarget(c.gameObject))
                {
                    targetEntities.Add(c.gameObject);
                }
            }
            return targetEntities;
        }
    }
}
