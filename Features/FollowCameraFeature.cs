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
            SetFeatureAttribute("moveSpeed", 5);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartFollow(GameObject target) {
            GetFeatureComponent<FollowPart>().FollowTarget(target);
        }

        public void StopFollow()
        {
            GetFeatureComponent<FollowPart>().Cancel();
        }
    }
}
