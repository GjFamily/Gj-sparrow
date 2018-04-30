using UnityEngine;

namespace Gj.Galaxy.Logic
{
    public enum RigidBodyParam : byte { Off, OnlyVelocity, OnlyAngularVelocity, All }

    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyView : MonoBehaviour, GameObservable
    {
        public RigidBodyParam rigidBodyParam = RigidBodyParam.All;

        Rigidbody m_Body;

        void Awake()
        {
            this.m_Body = GetComponent<Rigidbody>();
        }

        public void OnSerialize(StreamBuffer stream, MessageInfo info)
        {
            switch (this.rigidBodyParam)
            {
                case RigidBodyParam.All:
                    stream.SendNext(m_Body.velocity);
                    stream.SendNext(m_Body.angularVelocity);
                    break;
                case RigidBodyParam.OnlyAngularVelocity:
                    stream.SendNext(m_Body.angularVelocity);
                    break;
                case RigidBodyParam.OnlyVelocity:
                    stream.SendNext(m_Body.velocity);
                    break;
            }
        }

        public void OnDeserialize(StreamBuffer stream, MessageInfo info)
        {
            switch (this.rigidBodyParam)
            {
                case RigidBodyParam.All:
                    m_Body.velocity = stream.ReceiveNext<Vector3>();
                    m_Body.angularVelocity = stream.ReceiveNext<Vector3>();
                    break;
                case RigidBodyParam.OnlyAngularVelocity:
                    m_Body.angularVelocity = stream.ReceiveNext<Vector3>();
                    break;
                case RigidBodyParam.OnlyVelocity:
                    m_Body.velocity = stream.ReceiveNext<Vector3>();
                    break;
            }
        }

        public void Bind(NetworkEsse esse)
        {
        }
    }
}
