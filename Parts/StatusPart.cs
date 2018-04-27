using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    // TODO
    public class StatusPart : BasePart
    {
        private Dictionary<string, List<ExtraInfo>> extraInfoMap = new Dictionary<string, List<ExtraInfo>>();

        private void Start()
        {
            InvokeRepeating("Check", 0, 1);
        }

        private void OnDestroy()
        {
            CancelInvoke("Check");
        }

        public void AddExtra(ExtraInfo extraInfo)
        {
            if (!extraInfoMap.ContainsKey(extraInfo.name))
            {
                extraInfoMap.Add(extraInfo.name, new List<ExtraInfo>());
            }
            switch (extraInfo.extraType)
            {
                case ExtraInfo.ExtraType.Cast:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    AddAttribute(extraInfo);
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
            extraInfo.Ready();
            Merge(extraInfo);
        }

        public void CancelExtra(ExtraInfo extraInfo)
        {
            switch (extraInfo.extraType)
            {
                case ExtraInfo.ExtraType.Cast:
                    break;
                case ExtraInfo.ExtraType.Attribute:
                    CancelAttribute(extraInfo);
                    break;
                case ExtraInfo.ExtraType.Special:
                    break;
            }
            extraInfoMap[extraInfo.name].Remove(extraInfo);
        }

        private void AddAttribute(ExtraInfo extraInfo)
        {
            SetAttribute(extraInfo.attrubute, extraInfo.HandleAttribute(GetAttribute(extraInfo.attrubute)));
        }

        private void CancelAttribute(ExtraInfo extraInfo)
        {
            SetAttribute(extraInfo.attrubute, extraInfo.RecoveryAttribute(GetAttribute(extraInfo.attrubute)));
        }

        public void Merge(ExtraInfo extraInfo)
        {
            List<ExtraInfo> list = extraInfoMap[extraInfo.name];
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

        private void Check()
        {
            foreach (string key in extraInfoMap.Keys)
            {
                for (int i = 0; i < extraInfoMap[key].Count; i++)
                {
                    ExtraInfo extraInfo = extraInfoMap[key][i];
                    if (extraInfo.NeedCast())
                    {
                        Cast(extraInfo);
                    }
                    if (extraInfo.Over())
                    {
                        CancelExtra(extraInfo);
                        i--;
                    }
                }
            }
        }

        private void Cast(ExtraInfo extraInfo)
        {
            DefensePart defensePart = gameObject.GetComponent<DefensePart>();
            if (defensePart != null)
            {
                //defensePart.BeCast(extraInfo);
            }
        }
    }
}
