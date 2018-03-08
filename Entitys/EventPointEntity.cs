using UnityEngine;
using System.Collections;

namespace Gj
{
    public class EventPointEntity : BaseEntity
    {
        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                EventManage.single.Emit(name+"-enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                EventManage.single.Emit(name + "-exit");
            }
        }

        private bool IsPlayer(GameObject obj)
        {
            TargetEntity entity = obj.GetComponent<TargetEntity>();
            if (entity != null)
            {
                return entity.player;
            }
            else
            {
                return false;
            }
        }
    }
}
