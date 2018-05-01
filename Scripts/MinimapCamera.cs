using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MinimapCamera : MonoBehaviour
    {
        GameObject target;
        Vector3 offsetPosition;
        Quaternion offsetRotation;

		void Start()
        {
            offsetPosition = transform.position;
            offsetRotation = transform.rotation;
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
