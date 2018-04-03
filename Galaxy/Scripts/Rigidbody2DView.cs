using UnityEngine;

namespace Gj.Galaxy.Logic{
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class Rigidbody2DView : MonoBehaviour, GameObservable
    {

        public RigidBodyParam rigidBodyParam = RigidBodyParam.All;

        Rigidbody2D m_Body;

        void Awake()
        {
            this.m_Body = GetComponent<Rigidbody2D>();
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
                    m_Body.velocity = stream.ReceiveNext<Vector2>();
                    m_Body.angularVelocity = stream.ReceiveNext<float>();
                    break;
                case RigidBodyParam.OnlyAngularVelocity:
                    m_Body.angularVelocity = stream.ReceiveNext<float>();
                    break;
                case RigidBodyParam.OnlyVelocity:
                    m_Body.velocity = stream.ReceiveNext<Vector2>();
                    break;
            }
        }

        public void BindEntity(NetworkEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}