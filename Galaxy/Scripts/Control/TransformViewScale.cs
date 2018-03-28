using UnityEngine;
using System.Collections;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;

namespace Gj.Galaxy.Scripts{
    public class TransformViewScale
    {
        TransformOptions options;
        Extrapolated extrapolated;

        Vector3 m_NetworkScale = Vector3.one;

        public TransformViewScale(TransformOptions options, Extrapolated extrapolated)
        {
            this.options = options;
            this.extrapolated = extrapolated;
        }

        /// <summary>
        /// Gets the last scale that was received through the network
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNetworkScale()
        {
            return m_NetworkScale;
        }

        public Vector3 UpdateScale(Vector3 currentScale)
        {
            switch (options.scaleParam)
            {
                default:
                case ScaleParam.Disabled:
                    return m_NetworkScale;
                case ScaleParam.MoveTowards:
                    return Vector3.MoveTowards(currentScale, m_NetworkScale, options.scaleSpeed * Time.deltaTime);
                case ScaleParam.Lerp:
                    return Vector3.Lerp(currentScale, m_NetworkScale, options.scaleSpeed * Time.deltaTime);
            }
        }

        public void OnSerialize(Vector3 currentScale, StreamBuffer stream, MessageInfo info)
        {
            stream.Serialize(ref currentScale);
            m_NetworkScale = currentScale;
        }

        public void OnDeserialize(Vector3 currentScale, StreamBuffer stream, MessageInfo info){
            stream.DeSerialize(ref m_NetworkScale);
        }
    }
}
