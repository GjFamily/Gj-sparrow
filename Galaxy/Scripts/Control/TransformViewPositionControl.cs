using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Gj.Galaxy.Logic{
    public class TransformViewPositionControl
    {
        TransformViewPositionModel m_Model;
        float m_CurrentSpeed;
        double m_LastSerializeTime;
        Vector3 m_SynchronizedSpeed = Vector3.zero;
        float m_SynchronizedTurnSpeed = 0;

        Vector3 m_NetworkPosition;
        Queue<Vector3> m_OldNetworkPositions = new Queue<Vector3>();

        bool m_UpdatedPositionAfterOnSerialize = true;

        public TransformViewPositionControl(TransformViewPositionModel model)
        {
            m_Model = model;
        }

        Vector3 GetOldestStoredNetworkPosition()
        {
            Vector3 oldPosition = m_NetworkPosition;

            if (m_OldNetworkPositions.Count > 0)
            {
                oldPosition = m_OldNetworkPositions.Peek();
            }

            return oldPosition;
        }

        public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
        {
            m_SynchronizedSpeed = speed;
            m_SynchronizedTurnSpeed = turnSpeed;
        }

        public Vector3 UpdatePosition(Vector3 currentPosition)
        {
            Vector3 targetPosition = GetNetworkPosition() + GetExtrapolatedPositionOffset();

            switch (m_Model.InterpolateOption)
            {
                case TransformViewPositionModel.InterpolateOptions.Disabled:
                    if (m_UpdatedPositionAfterOnSerialize == false)
                    {
                        currentPosition = targetPosition;
                        m_UpdatedPositionAfterOnSerialize = true;
                    }
                    break;

                case TransformViewPositionModel.InterpolateOptions.FixedSpeed:
                    currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * m_Model.InterpolateMoveTowardsSpeed);
                    break;

                case TransformViewPositionModel.InterpolateOptions.EstimatedSpeed:
                    if (m_OldNetworkPositions.Count == 0)
                    {
                        break;
                    }

                    float estimatedSpeed = (Vector3.Distance(m_NetworkPosition, GetOldestStoredNetworkPosition()) / m_OldNetworkPositions.Count) * PeerClient.sendRateOnSerialize;

                    currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * estimatedSpeed);
                    break;

                case TransformViewPositionModel.InterpolateOptions.SynchronizeValues:
                    if (m_SynchronizedSpeed.magnitude == 0)
                    {
                        currentPosition = targetPosition;
                    }
                    else
                    {
                        currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * m_SynchronizedSpeed.magnitude);
                    }
                    break;

                case TransformViewPositionModel.InterpolateOptions.Lerp:
                    currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * m_Model.InterpolateLerpSpeed);
                    break;

                case TransformViewPositionModel.InterpolateOptions.MoveTowardsComplex:
                    float distanceToTarget = Vector3.Distance( currentPosition, targetPosition );
                    float targetSpeed = m_Model.InterpolateSpeedCurve.Evaluate( distanceToTarget ) * m_Model.InterpolateMoveTowardsSpeed;

                    if( targetSpeed > m_CurrentSpeed )
                    {
                        m_CurrentSpeed = Mathf.MoveTowards( m_CurrentSpeed, targetSpeed, Time.deltaTime * m_Model.InterpolateMoveTowardsAcceleration );
                    }
                    else
                    {
                        m_CurrentSpeed = Mathf.MoveTowards( m_CurrentSpeed, targetSpeed, Time.deltaTime * m_Model.InterpolateMoveTowardsDeceleration );
                    }

                    currentPosition = Vector3.MoveTowards( currentPosition, targetPosition, Time.deltaTime * m_CurrentSpeed );
                    break;
            }

            if (m_Model.TeleportEnabled == true)
            {
                if (Vector3.Distance(currentPosition, GetNetworkPosition()) > m_Model.TeleportIfDistanceGreaterThan)
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

        /// <summary>
        /// Calculates an estimated position based on the last synchronized position,
        /// the time when the last position was received and the movement speed of the object
        /// </summary>
        /// <returns>Estimated position of the remote object</returns>
        public Vector3 GetExtrapolatedPositionOffset()
        {
            float timePassed = (float)(PeerClient.time - m_LastSerializeTime);

            if (m_Model.ExtrapolateIncludingRoundTripTime == true)
            {
                timePassed += (float)PeerClient.GetPing() / 1000f;
            }

            Vector3 extrapolatePosition = Vector3.zero;

            switch (m_Model.ExtrapolateOption)
            {
                case TransformViewPositionModel.ExtrapolateOptions.SynchronizeValues:
                    Quaternion turnRotation = Quaternion.Euler(0, m_SynchronizedTurnSpeed * timePassed, 0);
                    extrapolatePosition = turnRotation * (m_SynchronizedSpeed * timePassed);
                    break;
                case TransformViewPositionModel.ExtrapolateOptions.FixedSpeed:
                    Vector3 moveDirection = (m_NetworkPosition - GetOldestStoredNetworkPosition()).normalized;

                    extrapolatePosition = moveDirection * m_Model.ExtrapolateSpeed * timePassed;
                    break;
                case TransformViewPositionModel.ExtrapolateOptions.EstimateSpeedAndTurn:
                    Vector3 moveDelta = (m_NetworkPosition - GetOldestStoredNetworkPosition()) * PeerClient.sendRateOnSerialize;
                    extrapolatePosition = moveDelta * timePassed;
                    break;
            }

            return extrapolatePosition;
        }

        public void OnSerialize(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            SerializeData(currentPosition, stream, info);

            m_LastSerializeTime = PeerClient.time;
            m_UpdatedPositionAfterOnSerialize = false;
        }

        public void OnDeserialize(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            DeserializeData(stream, info);

            m_LastSerializeTime = PeerClient.time;
            m_UpdatedPositionAfterOnSerialize = false;
        }

        void SerializeData(Vector3 currentPosition, StreamBuffer stream, MessageInfo info)
        {
            stream.SendNext(currentPosition);
            m_NetworkPosition = currentPosition;

            if (m_Model.ExtrapolateOption == TransformViewPositionModel.ExtrapolateOptions.SynchronizeValues ||
                m_Model.InterpolateOption == TransformViewPositionModel.InterpolateOptions.SynchronizeValues)
            {
                stream.SendNext(m_SynchronizedSpeed);
                stream.SendNext(m_SynchronizedTurnSpeed);
            }
        }

        void DeserializeData(StreamBuffer stream, MessageInfo info)
        {
            Vector3 readPosition = (Vector3)stream.ReceiveNext();
            if (m_Model.ExtrapolateOption == TransformViewPositionModel.ExtrapolateOptions.SynchronizeValues ||
                m_Model.InterpolateOption == TransformViewPositionModel.InterpolateOptions.SynchronizeValues)
            {
                m_SynchronizedSpeed = (Vector3)stream.ReceiveNext();
                m_SynchronizedTurnSpeed = (float)stream.ReceiveNext();
            }

            if (m_OldNetworkPositions.Count == 0)
            {
                // if we don't have old positions yet, this is the very first update this client reads. let's use this as current AND old position.
                m_NetworkPosition = readPosition;
            }

            // the previously received position becomes the old(er) one and queued. the new one is the m_NetworkPosition
            m_OldNetworkPositions.Enqueue(m_NetworkPosition);
            m_NetworkPosition = readPosition;

            // reduce items in queue to defined number of stored positions.
            while (m_OldNetworkPositions.Count > m_Model.ExtrapolateNumberOfStoredPositions)
            {
                m_OldNetworkPositions.Dequeue();
            }
        }
    }
}
