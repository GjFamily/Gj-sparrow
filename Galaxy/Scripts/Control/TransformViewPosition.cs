using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gj.Galaxy.Scripts;
using Gj.Galaxy.Logic;

namespace Gj.Galaxy.Scripts{
    public class TransformViewPosition
    {
        TransformOptions options;
        Extrapolated extrapolated;

        float m_CurrentSpeed;
        float m_SynchronizedSpeed = 0;

        Vector3 m_NetworkPosition;

        bool m_UpdatedPositionAfterOnSerialize = true;

        public TransformViewPosition(TransformOptions options, Extrapolated extrapolated)
        {
            this.options = options;
            this.extrapolated = extrapolated;
        }

        public void SetSynchronizedValues(float speed)
        {
            m_SynchronizedSpeed = speed;
        }

        public Vector3 UpdatePosition(Vector3 currentPosition)
        {
            Vector3 targetPosition = GetNetworkPosition() + extrapolated.GetPosition(m_CurrentSpeed);

            //UnityEngine.Debug.Log(targetPosition);
            switch (options.positionParam)
            {
                case PositionParam.Disabled:
                    if (m_UpdatedPositionAfterOnSerialize == false)
                    {
                        currentPosition = targetPosition;
                        m_UpdatedPositionAfterOnSerialize = true;
                    }
                    break;

                case PositionParam.FixedSpeed:
                    currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * options.positionSpeed);
                    break;

                //case PositionParam.EstimatedSpeed:
                    //if (m_OldNetworkPositions.Count == 0)
                    //{
                    //    break;
                    //}

                    //float estimatedSpeed = (Vector3.Distance(GetNetworkPosition(), GetOldestStoredNetworkPosition()) / m_OldNetworkPositions.Count) * PeerClient.sendRateOnSerialize;

                    //currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * estimatedSpeed);
                    //break;

                case PositionParam.SynchronizeValues:
                    if (m_SynchronizedSpeed == 0)
                    {
                        currentPosition = targetPosition;
                    }
                    else
                    {
                        currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * m_SynchronizedSpeed);
                    }
                    break;

                case PositionParam.Lerp:
                    currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * options.positionSpeed);
                    break;
                case PositionParam.MoveTowardsComplex:
                    float distanceToTarget = Vector3.Distance( currentPosition, targetPosition );
                    float targetSpeed = options.SpeedCurve.Evaluate( distanceToTarget ) * options.positionSpeed;

                    if( targetSpeed > m_CurrentSpeed )
                    {
                        m_CurrentSpeed = Mathf.MoveTowards( m_CurrentSpeed, targetSpeed, Time.deltaTime * options.Acceleration );
                    }
                    else
                    {
                        m_CurrentSpeed = Mathf.MoveTowards( m_CurrentSpeed, targetSpeed, Time.deltaTime * options.Deceleration );
                    }

                    currentPosition = Vector3.MoveTowards( currentPosition, targetPosition, Time.deltaTime * m_CurrentSpeed );
                    break;
            }

            if (options.TeleportEnabled == true)
            {
                if (Vector3.Distance(currentPosition, GetNetworkPosition()) > options.TeleportIfDistanceGreaterThan)
                {
                    currentPosition = GetNetworkPosition();
                }
            }

            return currentPosition;
        }

        public Vector3 GetNetworkPosition()
        {
            return m_NetworkPosition;
        }

        public void OnSerialize(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            SerializeData(currentPosition, stream, info);

            m_UpdatedPositionAfterOnSerialize = false;
        }

        public void OnDeserialize(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            DeserializeData(stream, info);

            m_UpdatedPositionAfterOnSerialize = false;
        }

        void SerializeData(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            stream.Serialize(ref currentPosition);
            m_NetworkPosition = currentPosition;

            if (options.positionParam == PositionParam.SynchronizeValues||
                options.extrapolatedParam == ExtrapolatedParam.SynchronizeValues)
            {
                stream.Serialize(ref m_SynchronizedSpeed);
            }
        }

        void DeserializeData(StreamBuffer stream, MessageInfo info)
        {
            stream.DeSerialize(ref m_NetworkPosition);
            if (options.positionParam == PositionParam.SynchronizeValues ||
                options.extrapolatedParam == ExtrapolatedParam.SynchronizeValues)
            {
                stream.DeSerialize(ref m_SynchronizedSpeed);
            }

            //if (m_OldNetworkPositions.Count == 0)
            //{
            //    // if we don't have old positions yet, this is the very first update this client reads. let's use this as current AND old position.
            //    m_NetworkPosition = readPosition;
            //}

            //// the previously received position becomes the old(er) one and queued. the new one is the m_NetworkPosition
            //m_OldNetworkPositions.Enqueue(m_NetworkPosition);
            //m_NetworkPosition = readPosition;

            //// reduce items in queue to defined number of stored positions.
            //while (m_OldNetworkPositions.Count > m_Model.ExtrapolateNumberOfStoredPositions)
            //{
            //    m_OldNetworkPositions.Dequeue();
            //}
            //}
            extrapolated.DeserializePosition(m_NetworkPosition);
        }
    }
}
