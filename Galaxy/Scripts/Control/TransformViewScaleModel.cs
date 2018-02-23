using UnityEngine;
using System.Collections;

namespace Gj.Galaxy.Logic{
    [System.Serializable]
    public class TransformViewScaleModel
    {
        public enum InterpolateOptions
        {
            Disabled,
            MoveTowards,
            Lerp,
        }

        public InterpolateOptions InterpolateOption = InterpolateOptions.Disabled;
        public float InterpolateMoveTowardsSpeed = 1f;
        public float InterpolateLerpSpeed;
    }
}
