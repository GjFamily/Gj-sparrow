using UnityEngine;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Utils;

namespace Gj.Galaxy.Scripts{
    [RequireComponent(typeof(NetworkEntity))]
    public class TransformView : MonoBehaviour, GameObservable
    {
        [SerializeField]
        public TransformViewPositionModel m_PositionModel = new TransformViewPositionModel();

        [SerializeField]
        public TransformViewRotationModel m_RotationModel = new TransformViewRotationModel();

        [SerializeField]
        public TransformViewScaleModel m_ScaleModel = new TransformViewScaleModel();

        public TransformParam transformParam = TransformParam.PositionAndRotation;

        TransformViewPositionControl m_PositionControl;
        TransformViewRotationControl m_RotationControl;
        TransformViewScaleControl m_ScaleControl;

        public bool SynchronizeEnabled;

        NetworkEntity m_entity;

        bool m_ReceivedNetworkUpdate = false;

        /// <summary>
        /// Flag to skip initial data when Object is instantiated and rely on the first deserialized data instead.
        /// </summary>
        bool m_firstTake = false;

        void Awake()
        {
            this.m_entity = GetComponent<NetworkEntity>();

            this.m_PositionControl = new TransformViewPositionControl(this.m_PositionModel);
            this.m_RotationControl = new TransformViewRotationControl(this.m_RotationModel);
            this.m_ScaleControl = new TransformViewScaleControl(this.m_ScaleModel);
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        void Update()
        {
            if (this.m_entity == null || this.m_entity.isMine == true || PeerClient.connected == false)
            {
                return;
            }
            transformUpdate();
        }

        void transformUpdate()
        {
            if (this.m_ReceivedNetworkUpdate == false && this.SynchronizeEnabled)
            {
                return;
            }
            switch (this.transformParam)
            {
                case TransformParam.All:
                    this.UpdatePosition();
                    this.UpdateRotation();
                    this.UpdateScale();
                    break;
                case TransformParam.OnlyPosition:
                    this.UpdatePosition();
                    break;
                case TransformParam.OnlyRotation:
                    this.UpdateRotation();
                    break;
                case TransformParam.OnlyScale:
                    this.UpdateScale();
                    break;
                case TransformParam.PositionAndRotation:
                    this.UpdatePosition();
                    this.UpdateRotation();
                    break;
            }

        }

        void UpdatePosition()
        {
            transform.localPosition = this.m_PositionControl.UpdatePosition(transform.localPosition);
        }

        void UpdateRotation()
        {
            transform.localRotation = this.m_RotationControl.GetRotation(transform.localRotation);
        }

        void UpdateScale()
        {
            transform.localScale = this.m_ScaleControl.GetScale(transform.localScale);
        }

        public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
        {
            this.m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
        }

        public void OnSerializeEntity(StreamBuffer stream, MessageInfo info)
        {
            switch (this.transformParam)
            {
                case TransformParam.All:
                    this.m_PositionControl.OnSerialize(transform.localPosition, stream, info);
                    this.m_RotationControl.OnSerialize(transform.localRotation, stream, info);
                    this.m_ScaleControl.OnSerialize(transform.localScale, stream, info);
                    break;
                case TransformParam.OnlyPosition:
                    this.m_PositionControl.OnSerialize(transform.localPosition, stream, info);
                    break;
                case TransformParam.OnlyRotation:
                    this.m_RotationControl.OnSerialize(transform.localRotation, stream, info);
                    break;
                case TransformParam.OnlyScale:
                    this.m_ScaleControl.OnSerialize(transform.localScale, stream, info);
                    break;
                case TransformParam.PositionAndRotation:
                    this.m_PositionControl.OnSerialize(transform.localPosition, stream, info);
                    this.m_RotationControl.OnSerialize(transform.localRotation, stream, info);
                    break;
            }

            if (this.m_entity.isMine == false && this.m_PositionModel.DrawErrorGizmo == true)
            {
                this.DoDrawEstimatedPositionError();
            }
        }

        public void OnDeserializeEntity(StreamBuffer stream, MessageInfo info)
        {
            switch (this.transformParam)
            {
                case TransformParam.All:
                    this.m_PositionControl.OnDeserialize(transform.localPosition, stream, info);
                    this.m_RotationControl.OnDeserialize(transform.localRotation, stream, info);
                    this.m_ScaleControl.OnDeserialize(transform.localScale, stream, info);
                    break;
                case TransformParam.OnlyPosition:
                    this.m_PositionControl.OnDeserialize(transform.localPosition, stream, info);
                    break;
                case TransformParam.OnlyRotation:
                    this.m_RotationControl.OnDeserialize(transform.localRotation, stream, info);
                    break;
                case TransformParam.OnlyScale:
                    this.m_ScaleControl.OnDeserialize(transform.localScale, stream, info);
                    break;
                case TransformParam.PositionAndRotation:
                    this.m_PositionControl.OnDeserialize(transform.localPosition, stream, info);
                    this.m_RotationControl.OnDeserialize(transform.localRotation, stream, info);
                    break;
            }

            if (this.m_entity.isMine == false && this.m_PositionModel.DrawErrorGizmo == true)
            {
                this.DoDrawEstimatedPositionError();
            }

            this.m_ReceivedNetworkUpdate = true;

            // force latest data to avoid initial drifts when player is instantiated.
            if (m_firstTake)
            {
                m_firstTake = false;

                transformUpdate();

            }
        }

        void DoDrawEstimatedPositionError()
        {
            Vector3 targetPosition = this.m_PositionControl.GetNetworkPosition();

            // we are synchronizing the localPosition, so we need to add the parent position for a proper positioning.
            if (transform.parent != null)
            {
                targetPosition = transform.parent.position + targetPosition;
            }

            Debug.DrawLine(targetPosition, transform.position, Color.red, 2f);
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.green, 2f);
            Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
        }

        public void SetSyncParam(byte param)
        {
            this.transformParam = (TransformParam)param;
        }
    } 
}
