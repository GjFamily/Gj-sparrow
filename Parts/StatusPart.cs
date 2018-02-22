using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class StatusPart : BasePart
    {
        private List<ExtraInfo> extraInfoList = new List<ExtraInfo>();
        private Action<ExtraInfo> AddAttributeNotic;
        private Action<ExtraInfo> CancelAttributeNotic;

        private void Start()
        {
            InvokeRepeating("Check", 0, 1);
        }

        private void OnDestroy()
        {
            CancelInvoke("Check");
        }

        public void SetAddNotic(Action<ExtraInfo> action) {
            AddAttributeNotic = action;
        }

        public void SetCancelNotic(Action<ExtraInfo> action)
        {
            CancelAttributeNotic = action;
        }

        public void AddExtra(ExtraInfo extraInfo)
        {
            switch (extraInfo.extraType)
            {
                case ExtraInfo.ExtraType.Cast:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    AddAttributeNotic(extraInfo);
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
            extraInfo.Ready();
            extraInfoList.Add(extraInfo);
        }

        public void CancelExtra(ExtraInfo extraInfo)
        {
            switch (extraInfo.extraType)
            {
                case ExtraInfo.ExtraType.Cast:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    CancelAttributeNotic(extraInfo);
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
            extraInfoList.Remove(extraInfo);
        }

        private void Check()
        {
            foreach(ExtraInfo extraInfo in extraInfoList) {
                if (extraInfo.NeedCast()) {
                    Cast(extraInfo);
                }
                if (extraInfo.Over()) {
                    CancelExtra(extraInfo);
                }
            }
        }

        private void Cast(ExtraInfo extraInfo)
        {
            DefensePart defensePart = gameObject.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                defensePart.BeCast(extraInfo);
            }
        }
    }
}
