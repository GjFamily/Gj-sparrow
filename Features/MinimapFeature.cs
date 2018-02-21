using UnityEngine;
using System.Collections;

namespace Gj
{
    public class MinimapFeature : BaseFeature
    {
        private bool followDirection;

        // Update is called once per frame
        void Update()
        {
            if (followDirection) {
                Model.transform.rotation = Quaternion.Euler(Model.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Model.transform.rotation.eulerAngles.z);
            }
        }

        public void NeedFollowDirection() {
            followDirection = true;
        }
    }
}
