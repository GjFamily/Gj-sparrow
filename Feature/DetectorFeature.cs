using System;
using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(RadarPart))]
    public class DetectorFeature : BaseFeature
    {
        protected new bool ignoreMaster = true;
        private Action<GameObject> FindPartner;
        private Action<GameObject> FindEnemy;

        protected override void Init()
        {
            base.Init();
            RadarPart radar = GetFeatureComponent<RadarPart>();
            radar.SetFindTargetNotic(FindTarget);
            radar.SetLoseTargetNotci(LoseTarget);
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
            RelationPart relation = GetMaster().GetComponent<RelationPart>();
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
