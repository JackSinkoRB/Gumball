using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(WheelCollider))]
    public class StanceModification : MonoBehaviour
    {

        private AICar carBelongsTo;

        private WheelCollider wheelCollider => GetComponent<WheelCollider>();
        private Transform wheelMeshCached;

        private Transform wheelMesh
        {
            get
            {
                if (wheelMeshCached == null)
                {
                    int index = carBelongsTo.AllWheelColliders.IndexOfItem(wheelCollider);
                    wheelMeshCached = carBelongsTo.AllWheelMeshes[index];
                }

                return wheelMeshCached;
            }
        }
        
        [Header("Modifiers")]
        [Tooltip("This is the actual value that is used for the suspension height on the wheel collider.")]
        [SerializeField] private RangedFloatValue suspensionHeight = new(0.01f, 0.2f, 0.05f);
        [Tooltip("This is the amount of rotation to apply to the wheels Z axis. This value is flipped for the left side wheels.")]
        [SerializeField] private RangedFloatValue camber = new(-5f, 15f, 0);
        [Tooltip("This is the wheels position on the X axis. This value is flipped for the left side wheels.")]
        [SerializeField] private RangedFloatValue offset = new(-0.1f, 0.1f, 0);
        [Tooltip("This value is multiplied to the initial wheel collider radius, as well as the meshes Y and Z scale axis'.")]
        [SerializeField] private RangedFloatValue diameter = new(0.7f, 1.1f, 1);
        [Tooltip("This value is multiplied to the initial wheel mesh X scale.")]
        [SerializeField] private RangedFloatValue width = new(0.5f, 1.5f, 1);

        public RangedFloatValue SuspensionHeight => suspensionHeight;
        public RangedFloatValue Camber => camber;
        public RangedFloatValue Offset => offset;
        public RangedFloatValue Diameter => diameter;
        public RangedFloatValue Width => width;

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;

            if (carBelongsTo.IsPlayerCar)
                ApplySavedPlayerData();
            else ApplyDefaultData();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //if application isn't playing, set all the default values on the car so it can be visualised
            if (!Application.isPlaying)
                ForceUpdateDefaultData();
        }

        private void ForceUpdateDefaultData()
        {
            //lazilly find the car reference if it hasn't been initialised
            if (carBelongsTo == null)
                carBelongsTo = transform.GetComponentInAllParents<AICar>();
            
            ApplyDefaultData();
            
            //update the meshes as if the car was driving
            carBelongsTo.UpdateWheelMeshes();
        }
#endif

        private void ApplyDefaultData()
        {
            ApplySuspensionHeight(suspensionHeight.DefaultValue);
        }

        private void ApplySavedPlayerData()
        {
            
        }

        public void ApplySuspensionHeight(float heightValue)
        {
            wheelCollider.suspensionDistance = heightValue;
        }

    }
}
