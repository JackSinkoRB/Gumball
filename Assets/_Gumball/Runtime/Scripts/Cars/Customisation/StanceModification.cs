using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    [RequireComponent(typeof(WheelCollider))]
    public class StanceModification : MonoBehaviour
    {

        [Header("Modifiers")]
        [SerializeField] private RangedFloatValue suspensionHeight = new(0.01f, 0.2f, 0.05f);
        [SerializeField] private RangedFloatValue camber = new(-5f, 15f, 0);
        [SerializeField] private RangedFloatValue offset = new(-0.1f, 0.1f, 0);
        [SerializeField] private RangedFloatValue tyreProfile = new(1, 1.3f, 1);
        [SerializeField] private RangedFloatValue tyreWidth = new(0.8f, 1, 1);
        [SerializeField] private RangedFloatValue rimDiameter = new(0.7f, 1.1f, 1);
        [SerializeField] private RangedFloatValue rimWidth = new(0.5f, 2f, 1);

        private AICar carBelongsTo;
        private int wheelIndex;
        public WheelMesh WheelMesh { get; private set; }
        
        private WheelCollider wheelCollider => GetComponent<WheelCollider>();
        private string saveKey => $"{carBelongsTo.SaveKey}.Wheel.{wheelIndex}";
        
        public RangedFloatValue SuspensionHeight => suspensionHeight;
        public RangedFloatValue Camber => camber;
        public RangedFloatValue Offset => offset;
        public RangedFloatValue TyreProfile => tyreProfile;
        public RangedFloatValue TyreWidth => tyreWidth;
        public RangedFloatValue RimDiameter => rimDiameter;
        public RangedFloatValue RimWidth => rimWidth;

        public float CurrentCamber { get; private set; }

        public void Initialise(AICar carBelongsTo)
        {
            this.carBelongsTo = carBelongsTo;
            
            FindWheelMesh();
            
            if (carBelongsTo.IsPlayer)
                ApplySavedPlayerData();
            else ApplyDefaultData();
        }

        private void FindWheelMesh()
        {
            wheelIndex = carBelongsTo.AllWheelColliders.IndexOfItem(wheelCollider);
            WheelMesh = carBelongsTo.AllWheelMeshes[wheelIndex];
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (!Application.isPlaying)
                ForceUpdateDefaultData();
        }
        
        private void OnValidate()
        {
            //if application isn't playing, set all the default values on the car so it can be visualised
            if (!Application.isPlaying)
                ForceUpdateDefaultData();
        }

        private void ForceUpdateDefaultData()
        {
            //lazilly find the car reference if it hasn't been initialised
            if (carBelongsTo == null || WheelMesh == null)
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
            ApplyTyreProfile(tyreProfile.DefaultValue);
            ApplyTyreWidth(tyreWidth.DefaultValue);
            ApplyRimDiameter(rimDiameter.DefaultValue);
            ApplyRimWidth(rimWidth.DefaultValue);
        }

        public void ApplySavedPlayerData()
        {
            //load values from file and apply them
            ApplySuspensionHeight(DataManager.Cars.Get($"{saveKey}.SuspensionHeight", suspensionHeight.DefaultValue));
            ApplyCamber(DataManager.Cars.Get($"{saveKey}.Camber", camber.DefaultValue));
            ApplyOffset(DataManager.Cars.Get($"{saveKey}.Offset", offset.DefaultValue));
            ApplyTyreProfile(DataManager.Cars.Get($"{saveKey}.TyreProfile", tyreProfile.DefaultValue));
            ApplyTyreWidth(DataManager.Cars.Get($"{saveKey}.TyreWidth", tyreWidth.DefaultValue));
            ApplyRimDiameter(DataManager.Cars.Get($"{saveKey}.RimDiameter", rimDiameter.DefaultValue));
            ApplyRimWidth(DataManager.Cars.Get($"{saveKey}.RimWidth", RimWidth.DefaultValue));
        }

        public void ApplySuspensionHeight(float heightValue)
        {
            wheelCollider.suspensionDistance = heightValue;

            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.SuspensionHeight", heightValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }
        
        public void ApplyCamber(float rotationValue)
        {
            CurrentCamber = rotationValue;
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.Camber", rotationValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }

        public void ApplyOffset(float offsetValue)
        {
            wheelCollider.transform.localPosition = wheelCollider.transform.localPosition.SetX(offsetValue);
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.Offset", offsetValue);
            
            carBelongsTo.UpdateWheelMeshes();
        }
        
        public void ApplyRimDiameter(float rimDiameterValue)
        {
            WheelMesh.transform.localScale = WheelMesh.transform.localScale.SetYZ(rimDiameterValue, rimDiameterValue);
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.RimDiameter", rimDiameterValue);
            
            UpdateWheelColliderRadius();
        }
        
        public void ApplyRimWidth(float rimWidthValue)
        {
            WheelMesh.transform.localScale = WheelMesh.transform.localScale.SetX(rimWidthValue);
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.RimWidth", rimWidthValue);
        }
                                
        public void ApplyTyreProfile(float tyreProfileValue)
        {
            if (WheelMesh.Tyre == null)
                return;
            
            WheelMesh.Tyre.transform.localScale = WheelMesh.Tyre.transform.localScale.SetXY(tyreProfileValue, tyreProfileValue);
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.TyreProfile", tyreProfileValue);
            
            WheelMesh.StretchTyre();
            UpdateWheelColliderRadius();
        }
        
        public void ApplyTyreWidth(float tyreWidthValue)
        {
            if (WheelMesh.Tyre == null)
                return;
            
            WheelMesh.Tyre.transform.localScale = WheelMesh.Tyre.transform.localScale.SetZ(tyreWidthValue);
            
            //save to file
            if (carBelongsTo.IsPlayer)
                DataManager.Cars.Set($"{saveKey}.TyreWidth", tyreWidthValue);
            
            WheelMesh.StretchTyre();
        }
        
        /// <summary>
        /// Adds the current camber to the wheel mesh rotation.
        /// </summary>
        public void AddCamberRotation()
        {
            if (WheelMesh == null)
                return; //not yet setup

            //add the camber
            WheelMesh.transform.Rotate(carBelongsTo.transform.forward, CurrentCamber, Space.World);
        }

        private void UpdateWheelColliderRadius()
        {
            MeshFilter wheelMesh = GetWheelMesh();

            if (wheelMesh == null)
                return;

            //the tyre is always larger than the wheel, so get the tyre extents (accounting for transform scales)
            Vector3 tyreSize = wheelMesh.transform.TransformPoint(wheelMesh.sharedMesh.bounds.extents) - wheelMesh.transform.position;
            
            //tyre is circular, so add the height and width and divide by 2. This is because we're working with world position and the tyre might be rotated in world space.
            wheelCollider.radius = (Mathf.Abs(tyreSize.y) + Mathf.Abs(tyreSize.z)) / 2f;
        }

        private MeshFilter GetWheelMesh()
        {
            if (WheelMesh.Tyre != null)
                return WheelMesh.Tyre.MeshFilter;

            if (WheelMesh.Rim != null && WheelMesh.Rim.Barrel != null)
                return WheelMesh.Rim.Barrel;

            return null;
        }
        
    }
}
