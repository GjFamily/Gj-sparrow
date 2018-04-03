﻿using System.Collections.Generic;
using Gj.Galaxy.Logic;
using UnityEngine;

namespace Gj.Galaxy.Scripts{
    [RequireComponent(typeof(Animator))]
    public class AnimatorView : MonoBehaviour, GameObservable
    {
        #region Enums

        public enum ParameterType
        {
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9,
        }

        public enum SynchronizeType
        {
            Disabled = 0,
            Discrete = 1,
            Continuous = 2,
        }

        [System.Serializable]
        public class SynchronizedParameter
        {
            public ParameterType Type;
            public SynchronizeType SynchronizeType;
            public string Name;
        }

        [System.Serializable]
        public class SynchronizedLayer
        {
            public SynchronizeType SynchronizeType;
            public int LayerIndex;
        }

        #endregion

        #region Properties

#if GALAXY_DEVELOP
    public AnimatorView ReceivingSender;
#endif

        #endregion

        #region Members

        private Animator m_Animator;

        private StreamQueue m_StreamQueue;

        //These fields are only used in the CustomEditor for this script and would trigger a
        //"this variable is never used" warning, which I am suppressing here
#pragma warning disable 0414
        [HideInInspector]
        [SerializeField]
        private bool ShowLayerWeightsInspector = true;

        [HideInInspector]
        [SerializeField]
        private bool ShowParameterInspector = true;
#pragma warning restore 0414

        [HideInInspector]
        [SerializeField]
        private List<SynchronizedParameter> m_SynchronizeParameters = new List<SynchronizedParameter>();

        [HideInInspector]
        [SerializeField]
        private List<SynchronizedLayer> m_SynchronizeLayers = new List<SynchronizedLayer>();

        private Vector3 m_ReceiverPosition;
        private float m_LastDeserializeTime;
        private bool m_WasSynchronizeTypeChanged = true;
        private NetworkEntity m_entity;

        List<string> m_raisedDiscreteTriggersCache = new List<string>();

        #endregion

        #region Unity

        private void Awake()
        {
            this.m_StreamQueue = new StreamQueue(120);

            this.m_Animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (this.m_Animator.applyRootMotion && this.m_entity.isMine == false)
            {
                this.m_Animator.applyRootMotion = false;
            }

            if (this.m_entity.isMine == true)
            {
                this.SerializeDataContinuously();

                this.CacheDiscreteTriggers();
            }
            else
            {
                this.DeserializeDataContinuously();
            }
        }

        #endregion

        #region Setup Synchronizing Methods

        /// <summary>
        /// Caches the discrete triggers values for keeping track of raised triggers, and will be reseted after the sync routine got performed
        /// </summary>
        public void CacheDiscreteTriggers()
        {
            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                SynchronizedParameter parameter = this.m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete && parameter.Type == ParameterType.Trigger && this.m_Animator.GetBool(parameter.Name))
                {
                    if (parameter.Type == ParameterType.Trigger)
                    {
                        this.m_raisedDiscreteTriggersCache.Add(parameter.Name);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check if a specific layer is configured to be synchronize
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>True if the layer is synchronized</returns>
        public bool DoesLayerSynchronizeTypeExist(int layerIndex)
        {
            return this.m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex) != -1;
        }

        /// <summary>
        /// Check if the specified parameter is configured to be synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>True if the parameter is synchronized</returns>
        public bool DoesParameterSynchronizeTypeExist(string name)
        {
            return this.m_SynchronizeParameters.FindIndex(item => item.Name == name) != -1;
        }

        /// <summary>
        /// Get a list of all synchronized layers
        /// </summary>
        /// <returns>List of SynchronizedLayer objects</returns>
        public List<SynchronizedLayer> GetSynchronizedLayers()
        {
            return this.m_SynchronizeLayers;
        }

        /// <summary>
        /// Get a list of all synchronized parameters
        /// </summary>
        /// <returns>List of SynchronizedParameter objects</returns>
        public List<SynchronizedParameter> GetSynchronizedParameters()
        {
            return this.m_SynchronizeParameters;
        }

        /// <summary>
        /// Gets the type how the layer is synchronized
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>Disabled/Discrete/Continuous</returns>
        public SynchronizeType GetLayerSynchronizeType(int layerIndex)
        {
            int index = this.m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex);

            if (index == -1)
            {
                return SynchronizeType.Disabled;
            }

            return this.m_SynchronizeLayers[index].SynchronizeType;
        }

        /// <summary>
        /// Gets the type how the parameter is synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>Disabled/Discrete/Continuous</returns>
        public SynchronizeType GetParameterSynchronizeType(string name)
        {
            int index = this.m_SynchronizeParameters.FindIndex(item => item.Name == name);

            if (index == -1)
            {
                return SynchronizeType.Disabled;
            }

            return this.m_SynchronizeParameters[index].SynchronizeType;
        }

        /// <summary>
        /// Sets the how a layer should be synchronized
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
        public void SetLayerSynchronized(int layerIndex, SynchronizeType synchronizeType)
        {
            if (Application.isPlaying == true)
            {
                this.m_WasSynchronizeTypeChanged = true;
            }

            int index = this.m_SynchronizeLayers.FindIndex(item => item.LayerIndex == layerIndex);

            if (index == -1)
            {
                this.m_SynchronizeLayers.Add(new SynchronizedLayer { LayerIndex = layerIndex, SynchronizeType = synchronizeType });
            }
            else
            {
                this.m_SynchronizeLayers[index].SynchronizeType = synchronizeType;
            }
        }

        /// <summary>
        /// Sets the how a parameter should be synchronized
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="synchronizeType">Disabled/Discrete/Continuous</param>
        public void SetParameterSynchronized(string name, ParameterType type, SynchronizeType synchronizeType)
        {
            if (Application.isPlaying == true)
            {
                this.m_WasSynchronizeTypeChanged = true;
            }

            int index = this.m_SynchronizeParameters.FindIndex(item => item.Name == name);

            if (index == -1)
            {
                this.m_SynchronizeParameters.Add(new SynchronizedParameter { Name = name, Type = type, SynchronizeType = synchronizeType });
            }
            else
            {
                this.m_SynchronizeParameters[index].SynchronizeType = synchronizeType;
            }
        }

        #endregion

        #region Serialization

        private void SerializeDataContinuously()
        {
            if (this.m_Animator == null)
            {
                return;
            }

            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                if (this.m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Continuous)
                {
                    this.m_StreamQueue.SendNext(this.m_Animator.GetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex));
                }
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                SynchronizedParameter parameter = this.m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Continuous)
                {
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            this.m_StreamQueue.SendNext(this.m_Animator.GetBool(parameter.Name));
                            break;
                        case ParameterType.Float:
                            this.m_StreamQueue.SendNext(this.m_Animator.GetFloat(parameter.Name));
                            break;
                        case ParameterType.Int:
                            this.m_StreamQueue.SendNext(this.m_Animator.GetInteger(parameter.Name));
                            break;
                        case ParameterType.Trigger:
                            this.m_StreamQueue.SendNext(this.m_Animator.GetBool(parameter.Name));
                            break;
                    }
                }
            }
        }


        private void DeserializeDataContinuously()
        {
            if (this.m_StreamQueue.HasQueuedObjects() == false)
            {
                return;
            }

            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                if (this.m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Continuous)
                {
                    this.m_Animator.SetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex, (float)this.m_StreamQueue.ReceiveNext());
                }
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                SynchronizedParameter parameter = this.m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Continuous)
                {
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            this.m_Animator.SetBool(parameter.Name, (bool)this.m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Float:
                            this.m_Animator.SetFloat(parameter.Name, (float)this.m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Int:
                            this.m_Animator.SetInteger(parameter.Name, (int)this.m_StreamQueue.ReceiveNext());
                            break;
                        case ParameterType.Trigger:
                            this.m_Animator.SetBool(parameter.Name, (bool)this.m_StreamQueue.ReceiveNext());
                            break;
                    }
                }
            }
        }

        private void SerializeDataDiscretly(StreamBuffer stream)
        {
            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                if (this.m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Discrete)
                {
                    stream.SendNext(this.m_Animator.GetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex));
                }
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                SynchronizedParameter parameter = this.m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete)
                {
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            stream.SendNext(this.m_Animator.GetBool(parameter.Name));
                            break;
                        case ParameterType.Float:
                            stream.SendNext(this.m_Animator.GetFloat(parameter.Name));
                            break;
                        case ParameterType.Int:
                            stream.SendNext(this.m_Animator.GetInteger(parameter.Name));
                            break;
                        case ParameterType.Trigger:
                            // here we can't rely on the current real state of the trigger, we might have missed its raise
                            stream.SendNext(this.m_raisedDiscreteTriggersCache.Contains(parameter.Name));
                            break;
                    }
                }
            }

            // reset the cache, we've synchronized.
            this.m_raisedDiscreteTriggersCache.Clear();
        }

        private void DeserializeDataDiscretly(StreamBuffer stream)
        {
            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                if (this.m_SynchronizeLayers[i].SynchronizeType == SynchronizeType.Discrete)
                {
                    this.m_Animator.SetLayerWeight(this.m_SynchronizeLayers[i].LayerIndex, (float)stream.ReceiveNext());
                }
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                SynchronizedParameter parameter = this.m_SynchronizeParameters[i];

                if (parameter.SynchronizeType == SynchronizeType.Discrete)
                {
                    switch (parameter.Type)
                    {
                        case ParameterType.Bool:
                            if (stream.PeekNext() is bool == false)
                            {
                                return;
                            }
                            this.m_Animator.SetBool(parameter.Name, (bool)stream.ReceiveNext());
                            break;
                        case ParameterType.Float:
                            if (stream.PeekNext() is float == false)
                            {
                                return;
                            }

                            this.m_Animator.SetFloat(parameter.Name, (float)stream.ReceiveNext());
                            break;
                        case ParameterType.Int:
                            if (stream.PeekNext() is int == false)
                            {
                                return;
                            }

                            this.m_Animator.SetInteger(parameter.Name, (int)stream.ReceiveNext());
                            break;
                        case ParameterType.Trigger:
                            if (stream.PeekNext() is bool == false)
                            {
                                return;
                            }

                            if ((bool)stream.ReceiveNext())
                            {
                                this.m_Animator.SetTrigger(parameter.Name);
                            }
                            break;
                    }
                }
            }
        }

        private void SerializeSynchronizationTypeState(StreamBuffer stream)
        {
            byte[] states = new byte[this.m_SynchronizeLayers.Count + this.m_SynchronizeParameters.Count];

            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                states[i] = (byte)this.m_SynchronizeLayers[i].SynchronizeType;
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                states[this.m_SynchronizeLayers.Count + i] = (byte)this.m_SynchronizeParameters[i].SynchronizeType;
            }

            stream.SendNext(states);
        }

        private void DeserializeSynchronizationTypeState(StreamBuffer stream)
        {
            byte[] state = (byte[])stream.ReceiveNext();

            for (int i = 0; i < this.m_SynchronizeLayers.Count; ++i)
            {
                this.m_SynchronizeLayers[i].SynchronizeType = (SynchronizeType)state[i];
            }

            for (int i = 0; i < this.m_SynchronizeParameters.Count; ++i)
            {
                this.m_SynchronizeParameters[i].SynchronizeType = (SynchronizeType)state[this.m_SynchronizeLayers.Count + i];
            }
        }

        public void OnSerializeEntity(StreamBuffer stream, MessageInfo info)
        {
            if (this.m_Animator == null)
            {
                return;
            }
            if (this.m_WasSynchronizeTypeChanged == true)
            {
                this.m_StreamQueue.Reset();
                this.SerializeSynchronizationTypeState(stream);

                this.m_WasSynchronizeTypeChanged = false;
            }

            this.m_StreamQueue.Serialize(stream);
            this.SerializeDataDiscretly(stream);
        }

        public void OnDeserializeEntity(StreamBuffer stream, MessageInfo info){

            #if GALAXY_DEVELOP
            if( ReceivingSender != null )
            {
                ReceivingSender.OnSerializeView( stream, info );
                break;
            }
            #endif
            {
                if (stream.PeekNext() is byte[])
                {
                    this.DeserializeSynchronizationTypeState(stream);
                }

                this.m_StreamQueue.Deserialize(stream);
                this.DeserializeDataDiscretly(stream);
            }
        }

        public void BindEntity(NetworkEntity entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
