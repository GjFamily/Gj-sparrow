using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MinimapCamera : MonoBehaviour
    {
        public float distance = 10;
        GameObject target;
        Vector3 offsetPosition;

		void Start()
		{
            offsetPosition = new Vector3(0, distance, 0);
		}

		// Update is called once per frame
		void Update()
        {
            if (target != null)
            {
                transform.position = target.transform.position + offsetPosition;
            }
        }

        public void StartFollow(GameObject obj) {
            target = obj;
        }

        public void StopFollow()
        {
            target = null;
        }
    }
}
