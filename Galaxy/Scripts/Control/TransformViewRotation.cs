using UnityEngine;
using System.Collections;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;

namespace Gj.Galaxy.Scripts{
    public class TransformViewRotation
    {
        TransformOptions options;
        Extrapolated extrapolated;

        Quaternion m_NetworkRotation;
        float m_SynchronizedTurnSpeed = 0;

        public TransformViewRotation(TransformOptions options, Extrapolated extrapolated)
        {
            this.options = options;
            this.extrapolated = extrapolated;
        }

        public Quaternion GetNetworkRotation()
        {
            return m_NetworkRotation;
        }

        public void SetSynchronizedValues(float turnSpeed)
        {
            m_SynchronizedTurnSpeed = turnSpeed;
        }

        public Quaternion UpdateRotation(Quaternion currentRotation)
        {
            Quaternion targetRotation = GetNetworkRotation();// * extrapolated.GetQuaternion(m_SynchronizedTurnSpeed);
            switch (options.rotationParam)
            {
                default:
                case RotationParam.Disabled:
                    return targetRotation;
                case RotationParam.RotateTowards:
                    return Quaternion.RotateTowards(currentRotation, targetRotation, options.rotationSpeed * Time.deltaTime);
                case RotationParam.SynchronizeValues:
                    if (m_SynchronizedTurnSpeed == 0)
                    {
                        return targetRotation;
                    }
                    else
                    {
                        return Quaternion.RotateTowards(currentRotation, targetRotation, m_SynchronizedTurnSpeed * Time.deltaTime);
                    }
                case RotationParam.Lerp:
                    return Quaternion.Lerp(currentRotation, targetRotation, options.rotationSpeed * Time.deltaTime);
            }
        }

        public void OnSerialize(Quaternion currentRotation, StreamBuffer stream, MessageInfo info)
        {
            stream.Serialize(ref currentRotation);
            m_NetworkRotation = currentRotation;
            if (options.rotationParam == RotationParam.SynchronizeValues||
                options.extrapolatedParam == ExtrapolatedParam.SynchronizeValues)
            {
                stream.Serialize(ref m_SynchronizedTurnSpeed);
            }
        }

        public void OnDeserialize(Quaternion currentRotation, StreamBuffer stream, MessageInfo info){
            stream.DeSerialize(ref m_NetworkRotation);
            if (options.rotationParam == RotationParam.SynchronizeValues ||
                options.extrapolatedParam == ExtrapolatedParam.SynchronizeValues)
            {
                stream.DeSerialize(ref m_SynchronizedTurnSpeed);
            }
            extrapolated.DeserializeRotation(m_NetworkRotation);
        }
    }
}
