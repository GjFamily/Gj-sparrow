using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gj.Galaxy.Logic;

namespace Gj.Galaxy.Scripts
{
    public class Extrapolated
    {
        private TransformOptions options;
        double m_LastSerializeTime;
        Vector3 m_LastPosition;
        Quaternion m_LastRotation;

        Queue<Vector3> m_OldPositions = new Queue<Vector3>();
        Queue<Quaternion> m_OldQuaternions = new Queue<Quaternion>();
        Vector3 GetOldestPosition()
        {
            if (m_OldPositions.Count > 0)
            {
                return m_OldPositions.Peek();
            }
            else
            {
                return m_LastPosition;
            }
        }
        Quaternion GetOldestRotation()
        {
            if (m_OldQuaternions.Count > 0)
            {
                return m_OldQuaternions.Peek();
            }
            else
            {
                return m_LastRotation;
            }
        }

        public Extrapolated(TransformOptions options)
        {
            this.options = options;
        }

        private float TimePassed()
        {
            float timePassed = (float)(PeerClient.time - m_LastSerializeTime);

            if (options.IncludingRoundTripTime == true)
            {
                timePassed += (float)PeerClient.PingTime / 1000f;
            }
            if (timePassed > options.IfTimeLesserThan * 2 * (float)PeerClient.PingTime / 1000f) return 0f;
            return timePassed;
        }

        public void DeserializePosition(Vector3 position)
        {
            m_LastPosition = position;
            m_LastSerializeTime = PeerClient.time;

            m_OldPositions.Enqueue(m_LastPosition);
            while (m_OldPositions.Count > options.NumberOfStoredPositions)
            {
                m_OldPositions.Dequeue();
            }
        }

        public Vector3 GetPosition(float speed)
        {
            var timePassed = TimePassed();
            Vector3 extrapolatePosition = Vector3.zero;
            Vector3 oldPosition = GetOldestPosition();
            if (m_LastPosition == oldPosition || timePassed == 0)
            {
                return extrapolatePosition;
            }
            Vector3 moveDirection = (m_LastPosition - oldPosition).normalized;
            switch (options.extrapolatedParam)
            {
                case ExtrapolatedParam.SynchronizeValues:
                    //Quaternion turnRotation = Quaternion.Euler(0, m_SynchronizedSpeed * timePassed, 0);
                    extrapolatePosition = moveDirection * (speed * timePassed);
                    break;
                case ExtrapolatedParam.FixedSpeed:
                    extrapolatePosition = moveDirection * options.extrapolatedPositionSpeed * timePassed;
                    break;
                case ExtrapolatedParam.EstimateSpeed:
                    Vector3 moveDelta = (m_LastPosition - oldPosition) * PeerClient.sendRateOnSerialize;
                    extrapolatePosition = moveDelta * timePassed;
                    break;
            }

            return extrapolatePosition;
        }

        public void DeserializeRotation(Quaternion rotation)
        {
            m_LastRotation = rotation;
            m_LastSerializeTime = PeerClient.time;

            m_OldQuaternions.Enqueue(m_LastRotation);
            while (m_OldQuaternions.Count > options.NumberOfStoredPositions)
            {
                m_OldQuaternions.Dequeue();
            }
        }

        public Quaternion GetQuaternion(float speed)
        {
            var timePassed = TimePassed();
            Quaternion extrapolateQuaternion = Quaternion.identity;
            Quaternion oldRotation = GetOldestRotation();
            if (m_LastRotation == oldRotation || timePassed == 0f)
            {
                return extrapolateQuaternion;
            }
            switch (options.extrapolatedParam)
            {
                case ExtrapolatedParam.SynchronizeValues:
                    Quaternion turnRotation = Quaternion.Euler(0, speed * timePassed, 0);
                    extrapolateQuaternion = turnRotation;
                    break;
                case ExtrapolatedParam.FixedSpeed:
                    extrapolateQuaternion = Quaternion.Euler(0, options.extrapolatedRatationSpeed * timePassed, 0);
                    break;
                //case ExtrapolatedParam.EstimateSpeed:
                    //Vector3 moveDelta = (m_LastRotation - oldRotation) * PeerClient.sendRateOnSerialize;
                    //extrapolateQuaternion = moveDelta * timePassed;
                    //break;
            }

            return extrapolateQuaternion;
        }
    }
}