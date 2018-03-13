using UnityEngine;
using System.Collections;

namespace Gj.Galaxy.Logic{
    public class TransformViewScaleControl
    {
        TransformViewScaleModel m_Model;
        Vector3 m_NetworkScale = Vector3.one;

        public TransformViewScaleControl(TransformViewScaleModel model)
        {
            m_Model = model;
        }

        /// <summary>
        /// Gets the last scale that was received through the network
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNetworkScale()
        {
            return m_NetworkScale;
        }

        public Vector3 GetScale(Vector3 currentScale)
        {
            switch (m_Model.InterpolateOption)
            {
                default:
                case TransformViewScaleModel.InterpolateOptions.Disabled:
                    return m_NetworkScale;
                case TransformViewScaleModel.InterpolateOptions.MoveTowards:
                    return Vector3.MoveTowards(currentScale, m_NetworkScale, m_Model.InterpolateMoveTowardsSpeed * Time.deltaTime);
                case TransformViewScaleModel.InterpolateOptions.Lerp:
                    return Vector3.Lerp(currentScale, m_NetworkScale, m_Model.InterpolateLerpSpeed * Time.deltaTime);
            }
        }

        public void OnSerialize(Vector3 currentScale, StreamBuffer stream, MessageInfo info)
        {
            stream.Serialize(ref currentScale);
            m_NetworkScale = currentScale;
        }

        public void OnDeserialize(Vector3 currentScale, StreamBuffer stream, MessageInfo info){
            stream.DeSerialize(out m_NetworkScale);
        }
    }
}
