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
                EventService.single.Emit(1);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsCurrentPlayer(other.gameObject))
            {
                EventService.single.Emit(1);
            }
        }

        private bool IsCurrentPlayer(GameObject obj)
        {
            Info info = CoreTools.GetInfo(obj);
            if (info != null)
            {
                return ObjectControl.Player == info.control;
            }
            else
            {
                return false;
            }
        }
    }
}
