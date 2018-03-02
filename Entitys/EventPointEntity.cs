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
