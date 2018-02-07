using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(Follow))]
    public class FollowCamera : BaseSub
    {
        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartFollow(GameObject target, float speed) {
            GetSubComponent<Follow>().FollowTarget(target, speed);
        }

        public void StopFollow()
        {
            GetSubComponent<Follow>().Cancel();
        }
    }
}
