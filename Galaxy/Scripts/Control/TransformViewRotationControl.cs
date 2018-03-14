using UnityEngine;
using System.Collections;

namespace Gj.Galaxy.Logic{
    public class TransformViewRotationControl
    {
        TransformViewRotationModel m_Model;
        Quaternion m_NetworkRotation;

        public TransformViewRotationControl(TransformViewRotationModel model)
        {
            m_Model = model;
        }

        public Quaternion GetNetworkRotation()
        {
            return m_NetworkRotation;
        }

        public Quaternion GetRotation(Quaternion currentRotation)
        {
            switch (m_Model.InterpolateOption)
            {
                default:
                case TransformViewRotationModel.InterpolateOptions.Disabled:
                    return m_NetworkRotation;
                case TransformViewRotationModel.InterpolateOptions.RotateTowards:
                    return Quaternion.RotateTowards(currentRotation, m_NetworkRotation, m_Model.InterpolateRotateTowardsSpeed * Time.deltaTime);
                case TransformViewRotationModel.InterpolateOptions.Lerp:
                    return Quaternion.Lerp(currentRotation, m_NetworkRotation, m_Model.InterpolateLerpSpeed * Time.deltaTime);
            }
        }

        public void OnSerialize(Quaternion currentRotation, StreamBuffer stream, MessageInfo info)
        {
            stream.Serialize(ref currentRotation);
            m_NetworkRotation = currentRotation;
        }

        public void OnDeserialize(Quaternion currentRotation, StreamBuffer stream, MessageInfo info){
            stream.DeSerialize(ref m_NetworkRotation);
        }
    }
}
