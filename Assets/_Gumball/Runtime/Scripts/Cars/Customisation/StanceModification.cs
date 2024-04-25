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

        private AICar carBelongsTo;
        private int wheelIndex;
        public Transform WheelMesh { get; private set; }
        
        private WheelCollider wheelCollider => GetComponent<WheelCollider>();
        private string saveKey => $"{carBelongsTo.SaveKey}.Wheel.{wheelIndex}";
        
        public RangedFloatValue SuspensionHeight => suspensionHeight;
        public RangedFloatValue Camber => camber;
        public RangedFloatValue Offset => offset;
        public RangedFloatValue Diameter => diameter;
        public RangedFloatValue Width => width;

        public float CurrentCamber { get; private set; }

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            FindWheelMesh();
            
            if (carBelongsTo.IsPlayerCar)
                ApplySavedPlayerData();
            else ApplyDefaultData();
        }

        private void FindWheelMesh()
        {
            wheelIndex = carBelongsTo.AllWheelColliders.IndexOfItem(wheelCollider);
            WheelMesh = carBelongsTo.AllWheelMeshes[wheelIndex];
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
            {
                carBelongsTo = transform.GetComponentInAllParents<AICar>();
                FindWheelMesh();
            }

            ApplyDefaultData();
            
            //update the meshes as if the car was driving
            carBelongsTo.UpdateWheelMeshes();
        }
#endif

        public void ApplyDefaultData()
        {
            ApplySuspensionHeight(suspensionHeight.DefaultValue);
            ApplyCamber(camber.DefaultValue);
            ApplyOffset(offset.DefaultValue);
        }

        public void ApplySavedPlayerData()
        {
            //load values from file and apply them
            ApplySuspensionHeight(DataManager.Cars.Get($"{saveKey}.SuspensionHeight", suspensionHeight.DefaultValue));
            ApplyCamber(DataManager.Cars.Get($"{saveKey}.Camber", camber.DefaultValue));
            ApplyOffset(DataManager.Cars.Get($"{saveKey}.Offset", offset.DefaultValue));
        }

        public void ApplySuspensionHeight(float heightValue)
        {
            wheelCollider.suspensionDistance = heightValue;

            //save to file
            if (carBelongsTo.IsPlayerCar)
                DataManager.Cars.Set($"{saveKey}.SuspensionHeight", heightValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }
        
        public void ApplyCamber(float rotationValue)
        {
            CurrentCamber = rotationValue;
            
            //save to file
            if (carBelongsTo.IsPlayerCar)
                DataManager.Cars.Set($"{saveKey}.Camber", rotationValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }

        public void ApplyOffset(float offsetValue)
        {
            wheelCollider.transform.localPosition = wheelCollider.transform.localPosition.SetX(offsetValue);
            
            //save to file
            if (carBelongsTo.IsPlayerCar)
                DataManager.Cars.Set($"{saveKey}.Offset", offsetValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }
        
        /// <summary>
        /// Adds the current camber to the wheel mesh rotation.
        /// </summary>
        public void AddCamberRotation()
        {
            if (WheelMesh == null)
                return; //not yet setup

            //add the camber
            WheelMesh.Rotate(carBelongsTo.transform.forward, CurrentCamber, Space.World);
        }

    }
}
