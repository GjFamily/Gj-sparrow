using UnityEngine;

namespace Gj.Galaxy.Logic{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyView : MonoBehaviour, GameObservable
    {
        public RigidBodyParam rigidBodyParam = RigidBodyParam.All;

        Rigidbody m_Body;

        void Awake()
        {
            this.m_Body = GetComponent<Rigidbody>();
        }

        public void OnSerializeEntity(StreamBuffer stream, MessageInfo info)
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

        public void OnDeserializeEntity(StreamBuffer stream, MessageInfo info)
        {
            switch (this.rigidBodyParam)
            {
                case RigidBodyParam.All:
                    m_Body.velocity = (Vector3)stream.ReceiveNext();
                    m_Body.angularVelocity = (Vector3)stream.ReceiveNext();
                    break;
                case RigidBodyParam.OnlyAngularVelocity:
                    m_Body.angularVelocity = (Vector3)stream.ReceiveNext();
                    break;
                case RigidBodyParam.OnlyVelocity:
                    m_Body.velocity = (Vector3)stream.ReceiveNext();
                    break;
            }
        }

        public void SetSyncParam(byte param)
        {
            this.rigidBodyParam = (RigidBodyParam)param;
        }
    }
}
