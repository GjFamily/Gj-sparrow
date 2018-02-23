using UnityEngine;
using System.Collections;

namespace Gj
{
    public class EventPointEntity : BaseEntity
    {
        public string eventKey;

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                EventManage.single.Emit(eventKey+"-enter");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                EventManage.single.Emit(eventKey + "-exit");
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
