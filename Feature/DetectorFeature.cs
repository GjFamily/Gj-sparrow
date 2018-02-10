using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(RadarPart))]
    public class DetectorFeature : BaseFeature
    {
        private Action<GameObject> FindPartner;
        private Action<GameObject> FindEnemy;

        void Start()
        {
            RadarPart radar = GetFeatureComponent<RadarPart>();
            radar.SetFindTargetNotic(FindTarget);
            radar.SetLoseTargetNotic(LoseTarget);
        }

        public void SetFindPartnerNotic(Action<GameObject> action)
        {
            FindPartner = action;
        }

        public void SetFindEnemyNotic(Action<GameObject> action)
        {
            FindEnemy = action;
        }

        private void FindTarget(GameObject obj)
        {
            RelationPart relation = GetComponent<RelationPart>();
            if (relation != null)
            {
                if (FindPartner != null && relation.IsPartner(obj))
                {
                    FindPartner(obj);
                }
                else if (FindEnemy != null && relation.IsEnemy(obj))
                {
                    FindEnemy(obj);
                }
            }
        }

        private void LoseTarget(GameObject obj)
        {

        }
    }
}
