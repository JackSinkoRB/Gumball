using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StanceWorkshopMenu : WorkshopSubMenu
    {
        
        public enum WheelsToModifyPosition
        {
            ALL,
            FRONT,
            REAR
        }
        
        [SerializeField] private Slider suspensionHeightSlider;
        [SerializeField] private Slider camberSlider;
        [SerializeField] private Slider offsetSlider;
        [SerializeField] private Slider tyreProfileSlider;
        [SerializeField] private Slider tyreWidthSlider;
        [SerializeField] private Slider rimDiameterSlider;
        [SerializeField] private Slider rimWidthSlider;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private WheelsToModifyPosition wheelsToModifyPosition;
        
        private WheelCollider[] wheelsToModify => wheelsToModifyPosition == WheelsToModifyPosition.ALL ? WarehouseManager.Instance.CurrentCar.AllWheelColliders
            : (wheelsToModifyPosition == WheelsToModifyPosition.FRONT ?
                WarehouseManager.Instance.CurrentCar.FrontWheelColliders : WarehouseManager.Instance.CurrentCar.RearWheelColliders);
        
        private void OnEnable()
        {
            UpdateSliderValues();
        }

        /// <summary>
        /// Set which wheels will be modified when using the sliders.
        /// </summary>
        /// <param name="position"></param>
        public void SetWheelsToModifyPosition(WheelsToModifyPosition position)
        {
            wheelsToModifyPosition = position;
            
            UpdateSliderValues();
        }

        public void SetWheelsToModifyPosition(int index) => SetWheelsToModifyPosition((WheelsToModifyPosition)index);
        
        /// <summary>
        /// Updates the slider values with the cars current values.
        /// </summary>
        private void UpdateSliderValues()
        {
            WheelCollider wheelToUse = wheelsToModify[0]; //just use the first wheel
            
            StanceModification stanceModification = wheelToUse.GetComponent<StanceModification>();

            float suspensionHeightNormalized = stanceModification.SuspensionHeight.NormalizeValue(wheelToUse.suspensionDistance);
            suspensionHeightSlider.SetValueWithoutNotify(suspensionHeightNormalized);
            
            float camberNormalized = stanceModification.Camber.NormalizeValue(stanceModification.CurrentCamber);
            camberSlider.SetValueWithoutNotify(camberNormalized);
            
            float offsetNormalized = stanceModification.Offset.NormalizeValue(wheelToUse.transform.localPosition.x);
            offsetSlider.SetValueWithoutNotify(offsetNormalized);
            
            float tyreWidthNormalized = stanceModification.TyreWidth.NormalizeValue(stanceModification.WheelMesh.Tyre.transform.localScale.z);
            tyreWidthSlider.SetValueWithoutNotify(tyreWidthNormalized);
            
            float rimDiameterNormalized = stanceModification.RimDiameter.NormalizeValue(stanceModification.WheelMesh.transform.localScale.y);
            rimDiameterSlider.SetValueWithoutNotify(rimDiameterNormalized);
            
            float rimWidthNormalized = stanceModification.RimWidth.NormalizeValue(stanceModification.WheelMesh.transform.localScale.x);
            rimWidthSlider.SetValueWithoutNotify(rimWidthNormalized);
        }

        public void OnSuspensionHeightSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

                float valueDenormalized = stanceModification.SuspensionHeight.DenormalizeValue(suspensionHeightSlider.value);
                stanceModification.ApplySuspensionHeight(valueDenormalized);
            }
        }
        
        public void OnCamberSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.Camber.DenormalizeValue(camberSlider.value);
                stanceModification.ApplyCamber(valueDenormalized);
            }
        }
        
        public void OnOffsetSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.Offset.DenormalizeValue(offsetSlider.value);
                stanceModification.ApplyOffset(valueDenormalized);
            }
        }
                
        public void OnTyreProfileSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.TyreProfile.DenormalizeValue(tyreProfileSlider.value);
                stanceModification.ApplyTyreProfile(valueDenormalized);
            }
        }
        
        public void OnTyreWidthSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.TyreWidth.DenormalizeValue(tyreWidthSlider.value);
                stanceModification.ApplyTyreWidth(valueDenormalized);
            }
        }
        
        public void OnRimDiameterSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.RimDiameter.DenormalizeValue(rimDiameterSlider.value);
                stanceModification.ApplyRimDiameter(valueDenormalized);
            }
        }
        
        public void OnRimWidthSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.RimWidth.DenormalizeValue(rimWidthSlider.value);
                stanceModification.ApplyRimWidth(valueDenormalized);
            }
        }
        
    }
}
