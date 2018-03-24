using UnityEngine;
using System.Collections;

namespace Gj
{
    public class EventPointEntity : BaseEntity
    {
        private void OnTriggerEnter(Collider other)
        {
            if (IsCurrentPlayer(other.gameObject))
            {
                EventManage.single.Emit(name+"-enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsCurrentPlayer(other.gameObject))
            {
                EventManage.single.Emit(name + "-exit");
            }
        }

        private bool IsCurrentPlayer(GameObject obj)
        {
            Info info = obj.GetComponent<Info>();
            if (info != null)
            {
                return info.currentPlayer;
            }
            else
            {
                return false;
            }
        }
    }
}
