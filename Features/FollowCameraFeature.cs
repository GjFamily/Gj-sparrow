using UnityEngine;
using System.Collections;

namespace Gj
{
    [RequirePart(typeof(FollowPart))]
    public class FollowCameraFeature : BaseFeature
    {

        float r;

        // Use this for initialization
        void Start()
        {
            SetFeatureAttribute("moveSpeed", 5);
            r = Feature.transform.position.y / Feature.transform.position.z;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetDistance (float d) {
            GetFeatureComponent<FollowPart>().SetOffset(new Vector3(0, d, d / r));
        }

        public void StartFollow(GameObject target) {
            GetFeatureComponent<FollowPart>().FollowTarget(target, Feature.transform.position);
        }

        public void StopFollow()
        {
            GetFeatureComponent<FollowPart>().Cancel();
        }
    }
}
