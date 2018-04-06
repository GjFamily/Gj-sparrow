using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(FollowPart))]
    public class MinimapCameraFeature : BaseFeature
    {
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartFollow(GameObject target) {
            GetFeatureComponent<FollowPart>().SetOffset(new Vector3(0, 15, 0));
            GetFeatureComponent<FollowPart>().FollowTarget(target);
        }

        public void StopFollow()
        {
            GetFeatureComponent<FollowPart>().Cancel();
        }
    }
}
