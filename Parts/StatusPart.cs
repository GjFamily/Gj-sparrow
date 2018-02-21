using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class StatusPart : BasePart
    {
        private Dictionary<ExtraInfo.ExtraType, List<ExtraInfo>> extraInfoMap = new Dictionary<ExtraInfo.ExtraType, List<ExtraInfo>>();

        public void AddExtra (ExtraInfo extraInfo, GameObject obj) {
            if (!extraInfoMap.ContainsKey(extraInfo.extraType)) {
                extraInfoMap.Add(extraInfo.extraType, new List<ExtraInfo>());
            }
            extraInfoMap[extraInfo.extraType].Add(extraInfo);
            switch (extraInfo.extraType) {
                case ExtraInfo.ExtraType.Damage:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
        }

        public void CancelExtra (ExtraInfo extraInfo, GameObject obj) {
            switch (extraInfo.extraType)
            {
                case ExtraInfo.ExtraType.Damage:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
            extraInfoMap[extraInfo.extraType].Remove(extraInfo);
        }

        private void CheckDamage () {
            
        }

        private void HandleAttribute () {
            
        }

        private void RecoveryAttribute()
        {

        }

        private void Damaged (ExtraInfo extraInfo, GameObject obj) {
            DefensePart defensePart = gameObject.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                defensePart.BeAttacked(extraInfo, obj);
            }
        }
    }
}
