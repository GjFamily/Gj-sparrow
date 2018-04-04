using UnityEngine;
using System.Collections;
using Gj;

namespace Gj {
    [RequirePart(typeof(RotatePart))]
    public class RotateFeature : BaseFeature
    {
        public void SetAngle(float angle) {
            GetFeatureComponent<RotatePart>().SetRotateAngle(angle);
        }

        public void SetSpeed(float speed) {
            SetFeatureAttribute("rotateSpeed", speed);
        }
    }
}
