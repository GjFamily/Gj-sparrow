using UnityEngine;
using System.Collections;

namespace Gj
{
    public class AttackPart : BasePart
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Launch(GameObject skill, GameObject start, float power, float distance, float speed) {
            Vector3 p = new Vector3(start.transform.position.x, 0, start.transform.position.z) + start.transform.forward * distance;
            SkillEntity skillEntity = skill.GetComponent<SkillEntity>();
            if (skillEntity!=null) {
                skillEntity.SetMaster(gameObject, power).Cast(start.transform.position, p, speed);
            }
        }
    }
}
