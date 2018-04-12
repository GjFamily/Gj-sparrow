using UnityEngine;
using System.Collections.Generic;

namespace Gj
{
    public class ResControl : BaseControl
    {
        private void OnTriggerEnter(Collider other)
        {
            if (IsCurrentPlayer(other.gameObject))
            {
                EventService.single.Emit(name + "-enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsCurrentPlayer(other.gameObject))
            {
                EventService.single.Emit(name + "-exit");
            }
        }

        private bool IsCurrentPlayer(GameObject obj)
        {
            Info info = CoreTools.GetInfo(obj);
            if (info != null)
            {
                return info.player;
            }
            else
            {
                return false;
            }
        }
    }
}
