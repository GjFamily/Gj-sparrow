using UnityEngine;
using System.Collections;

namespace Gj.Galaxy.Logic{
    [System.Serializable]
    public class TransformViewRotationModel
    {
        public enum InterpolateOptions
        {
            Disabled,
            RotateTowards,
            Lerp,
        }

        public InterpolateOptions InterpolateOption = InterpolateOptions.Lerp;
        public float InterpolateRotateTowardsSpeed = 180;
        public float InterpolateLerpSpeed = 5;
    }
}
