using UnityEngine;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Utils;
using System.Collections.Generic;

namespace Gj.Galaxy.Scripts
{
    public enum TransformParam : byte { Off, OnlyPosition, OnlyRotation, OnlyScale, PositionAndRotation, All }
    public enum ExtrapolatedParam : byte { Disabled, SynchronizeValues, EstimateSpeed, FixedSpeed, }
    public enum PositionParam : byte
    {
        Disabled,
        FixedSpeed,
        //EstimatedSpeed,
        SynchronizeValues,
        MoveTowardsComplex,
        Lerp,
    }
    public enum RotationParam : byte
    {
        Disabled,
        FixedSpeed,
        SynchronizeValues,
        Lerp,
    }
    public enum ScaleParam : byte
    {
        Disabled,
        MoveTowards,
        Lerp,
    }

    public class TransformView : MonoBehaviour, GameObservable
    {
        public TransformParam transformParam = TransformParam.PositionAndRotation;
        public TransformOptions options = new TransformOptions();

        TransformViewPosition positionControl;
        TransformViewRotation rotationControl;
        TransformViewScale scaleControl;

        Extrapolated extrapolated;

        NetworkEsse esse;

        bool receivedNetworkUpdate = false;

        /// <summary>
        /// Flag to skip initial data when Object is instantiated and rely on the first deserialized data instead.
        /// </summary>
        bool m_firstTake = false;

        void Awake()
        {
            this.extrapolated = new Extrapolated(this.options);

            this.positionControl = new TransformViewPosition(this.options, this.extrapolated);
            this.rotationControl = new TransformViewRotation(this.options, this.extrapolated);
            this.scaleControl = new TransformViewScale(this.options, this.extrapolated);

        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        void Update()
        {
            if (this.esse == null || this.esse.isMine == true || PeerClient.connected == false)
            {
                return;
            }
            transformUpdate();
        }

        void transformUpdate()
        {
            if (this.receivedNetworkUpdate == false)
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
            transform.localPosition = this.positionControl.UpdatePosition(transform.localPosition);
        }

        void UpdateRotation()
        {
            transform.localRotation = this.rotationControl.UpdateRotation(transform.localRotation);
        }

        void UpdateScale()
        {
            transform.localScale = this.scaleControl.UpdateScale(transform.localScale);
        }

        public void SetSynchronizedValues(float speed, float turnSpeed)
        {
            this.positionControl.SetSynchronizedValues(speed);
            this.rotationControl.SetSynchronizedValues(turnSpeed);
        }

        public void OnSerialize(StreamBuffer stream, MessageInfo info)
        {
            switch (this.transformParam)
            {
                case TransformParam.All:
                    this.positionControl.OnSerialize(transform.localPosition, stream, info);
                    this.rotationControl.OnSerialize(transform.localRotation, stream, info);
                    this.scaleControl.OnSerialize(transform.localScale, stream, info);
                    break;
                case TransformParam.OnlyPosition:
                    this.positionControl.OnSerialize(transform.localPosition, stream, info);
                    break;
                case TransformParam.OnlyRotation:
                    this.rotationControl.OnSerialize(transform.localRotation, stream, info);
                    break;
                case TransformParam.OnlyScale:
                    this.scaleControl.OnSerialize(transform.localScale, stream, info);
                    break;
                case TransformParam.PositionAndRotation:
                    this.positionControl.OnSerialize(transform.localPosition, stream, info);
                    this.rotationControl.OnSerialize(transform.localRotation, stream, info);
                    break;
            }

            if (this.esse.isMine == false && this.options.DrawErrorGizmo == true)
            {
                this.DoDrawEstimatedPositionError();
            }
        }

        public void OnDeserialize(StreamBuffer stream, MessageInfo info)
        {
            switch (this.transformParam)
            {
                case TransformParam.All:
                    this.positionControl.OnDeserialize(transform.localPosition, stream, info);
                    this.rotationControl.OnDeserialize(transform.localRotation, stream, info);
                    this.scaleControl.OnDeserialize(transform.localScale, stream, info);
                    break;
                case TransformParam.OnlyPosition:
                    this.positionControl.OnDeserialize(transform.localPosition, stream, info);
                    break;
                case TransformParam.OnlyRotation:
                    this.rotationControl.OnDeserialize(transform.localRotation, stream, info);
                    break;
                case TransformParam.OnlyScale:
                    this.scaleControl.OnDeserialize(transform.localScale, stream, info);
                    break;
                case TransformParam.PositionAndRotation:
                    this.positionControl.OnDeserialize(transform.localPosition, stream, info);
                    this.rotationControl.OnDeserialize(transform.localRotation, stream, info);
                    break;
            }

            if (this.esse.isMine == false && this.options.DrawErrorGizmo == true)
            {
                this.DoDrawEstimatedPositionError();
            }

            this.receivedNetworkUpdate = true;

            // force latest data to avoid initial drifts when player is instantiated.
            if (m_firstTake)
            {
                m_firstTake = false;

                transformUpdate();

            }
        }

        void DoDrawEstimatedPositionError()
        {
            Vector3 targetPosition = this.positionControl.GetNetworkPosition();

            // we are synchronizing the localPosition, so we need to add the parent position for a proper positioning.
            if (transform.parent != null)
            {
                targetPosition = transform.parent.position + targetPosition;
            }

            Debug.DrawLine(targetPosition, transform.position, Color.red, 2f);
            Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.green, 2f);
            Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
        }

        public void Bind(NetworkEsse esse)
        {
            this.esse = esse;
        }
    }

    public class TransformOptions
    {

        public bool TeleportEnabled = true;
        public float TeleportIfDistanceGreaterThan = 3f;

        public PositionParam positionParam = PositionParam.SynchronizeValues;
        public float positionSpeed = 3f;

        public float Acceleration = 2;
        public float Deceleration = 2;
        public AnimationCurve SpeedCurve = new AnimationCurve(
            new Keyframe[] {
              new Keyframe( -1, 0, 0, Mathf.Infinity ),
              new Keyframe( 0, 1, 0, 0 ),
              new Keyframe( 1, 1, 0, 1 ),
              new Keyframe( 4, 4, 1, 0 )
            }
        );

        public RotationParam rotationParam = RotationParam.FixedSpeed;
        public float rotationSpeed = 180;

        public ScaleParam scaleParam = ScaleParam.Disabled;
        public float scaleSpeed = 1f;

        public ExtrapolatedParam extrapolatedParam = ExtrapolatedParam.SynchronizeValues;
        public float extrapolatedPositionSpeed = 3f;
        public float extrapolatedRatationSpeed = 180f;

        public bool IncludingRoundTripTime = true;
        public int NumberOfStoredPositions = 1;
        public int IfTimeLesserThan = 2;

        //public bool DrawNetworkGizmo = true;
        //public Color NetworkGizmoColor = Color.red;
        //public ExitGames.Client.GUI.GizmoType NetworkGizmoType;
        //public float NetworkGizmoSize = 1f;

        //public bool DrawExtrapolatedGizmo = true;
        //public Color ExtrapolatedGizmoColor = Color.yellow;
        //public ExitGames.Client.GUI.GizmoType ExtrapolatedGizmoType;
        //public float ExtrapolatedGizmoSize = 1f;

        public bool DrawErrorGizmo = true;
    }

}
