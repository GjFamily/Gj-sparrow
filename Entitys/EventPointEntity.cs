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
                EventManage.single.Emit(objName+"-enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                EventManage.single.Emit(objName + "-exit");
            }
        }

        private bool IsPlayer(GameObject obj)
        {
            BaseEntity entity = obj.GetComponent<BaseEntity>();
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
