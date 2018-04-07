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
            RelationPart relation = GetComponent<RelationPart>();
            if (relation != null)
            {
                foreach (GameObject t in targetEntities)
                {
                    if (relation.IsEnemy(t.gameObject))
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
            }
            return enemy;
        }

        public List<GameObject> FindEnemy()
        {
            List<GameObject> targetEntities = FindTarget();
            List<GameObject> enemys = new List<GameObject>();

            RelationPart relation = GetComponent<RelationPart>();
            if (relation != null)
            {
                foreach (GameObject t in targetEntities)
                {
                    if (relation.IsEnemy(t.gameObject))
                    {
                        enemys.Add(t);
                    }
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
            RelationPart relation = GetComponent<RelationPart>();
            if (relation != null)
            {
                foreach (GameObject t in targetEntities)
                {
                    if (relation.IsPartner(t.gameObject))
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
            }
            return enemy;
        }

        public List<GameObject> FindPartner()
        {
            List<GameObject> targetEntities = FindTarget();
            List<GameObject> partners = new List<GameObject>();

            RelationPart relation = GetComponent<RelationPart>();
            if (relation != null)
            {
                foreach (GameObject t in targetEntities)
                {
                    if (relation.IsPartner(t.gameObject))
                    {
                        partners.Add(t);
                    }
                }
            }
            return partners;
        }

        public List<GameObject> FindTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, GetAttribute("detectorRadius"));
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
