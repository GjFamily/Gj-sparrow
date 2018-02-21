using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(FollowPart))]
    public class FollowCameraFeature : BaseFeature
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
            GetFeatureComponent<FollowPart>().FollowTarget(target, speed);
        }

        public void StopFollow()
        {
            GetFeatureComponent<FollowPart>().Cancel();
        }
    }
}
