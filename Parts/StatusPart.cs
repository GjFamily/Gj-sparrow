using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class StatusPart : BasePart
    {
        private Dictionary<string, List<ExtraInfo>> extraInfoMap = new Dictionary<string, List<ExtraInfo>>();
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

        public void SetAddNotic(Action<ExtraInfo> action)
        {
            AddAttributeNotic = action;
        }

        public void SetCancelNotic(Action<ExtraInfo> action)
        {
            CancelAttributeNotic = action;
        }

        public void AddExtra(ExtraInfo extraInfo)
        {
            if (!extraInfoMap.ContainsKey(extraInfo.skillName))
            {
                extraInfoMap.Add(extraInfo.skillName, new List<ExtraInfo>());
            }
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
            Merge(extraInfo);
        }

        public void Merge(ExtraInfo extraInfo)
        {
            List<ExtraInfo> list = extraInfoMap[extraInfo.skillName];
            switch (extraInfo.numType)
            {
                case ExtraInfo.NumType.Only:
                    if (list.Count > 0)
                    {
                        list[0].Refresh();
                    }
                    else
                    {
                        list.Add(extraInfo);
                    }
                    break;
                case ExtraInfo.NumType.TargetOnly:
                    if (list.Count > 0)
                    {
                        ExtraInfo target = null;
                        foreach (ExtraInfo e in list)
                        {
                            if (e.master == extraInfo.master)
                            {
                                target = e;
                            }
                        }
                        if (target != null)
                        {
                            target.Refresh();
                        }
                        else
                        {
                            list.Add(extraInfo);
                        }
                    }
                    else
                    {
                        list.Add(extraInfo);
                    }
                    break;
                case ExtraInfo.NumType.None:
                    list.Add(extraInfo);
                    break;
            }
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
            extraInfoMap[extraInfo.skillName].Remove(extraInfo);
        }

        private void Check()
        {
            foreach (string key in extraInfoMap.Keys)
            {
                foreach (ExtraInfo extraInfo in extraInfoMap[key])
                {
                    if (extraInfo.NeedCast())
                    {
                        Cast(extraInfo);
                    }
                    if (extraInfo.Over())
                    {
                        CancelExtra(extraInfo);
                    }
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
