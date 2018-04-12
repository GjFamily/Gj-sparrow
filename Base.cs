using UnityEngine;
using System.Collections;

namespace Gj
{
    public interface UISystem
    {
        void UIClick(string key);
    }

    public enum TargetRelation
    {
        Self,
        Partner,
        Enemy
    }
}
