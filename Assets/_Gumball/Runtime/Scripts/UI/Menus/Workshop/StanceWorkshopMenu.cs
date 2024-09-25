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

        [Space(5)]
        [SerializeField] private Button[] wheelModificationButtons;
        [Space(5)]
        [SerializeField] private SliderWithPercent suspensionHeightSlider;
        [SerializeField] private SliderWithPercent camberSlider;
        [SerializeField] private SliderWithPercent offsetSlider;
        [SerializeField] private SliderWithPercent tyreProfileSlider;
        [SerializeField] private SliderWithPercent tyreWidthSlider;
        [SerializeField] private SliderWithPercent rimDiameterSlider;
        [SerializeField] private SliderWithPercent rimWidthSlider;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private WheelsToModifyPosition wheelsToModifyPosition;
        
        private WheelCollider[] wheelsToModify => wheelsToModifyPosition == WheelsToModifyPosition.ALL ? WarehouseManager.Instance.CurrentCar.AllWheelColliders
            : (wheelsToModifyPosition == WheelsToModifyPosition.FRONT ?
                WarehouseManager.Instance.CurrentCar.FrontWheelColliders : WarehouseManager.Instance.CurrentCar.RearWheelColliders);

        protected override void OnShow()
        {
            base.OnShow();

            SetWheelsToModifyPosition(WheelsToModifyPosition.ALL);
        }

        /// <summary>
        /// Set which wheels will be modified when using the sliders.
        /// </summary>
        public void SetWheelsToModifyPosition(WheelsToModifyPosition position)
        {
            wheelsToModifyPosition = position;
            
            UpdateSliderValues();

            for (int index = 0; index < wheelModificationButtons.Length; index++)
            {
                Button button = wheelModificationButtons[index];
                WheelsToModifyPosition buttonPosition = (WheelsToModifyPosition)index;
                
                bool isSelected = buttonPosition == position;
                //TODO: tween the button color depending on selected or not
            }
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
            suspensionHeightSlider.UpdateSlider(suspensionHeightNormalized);
            
            float camberNormalized = stanceModification.Camber.NormalizeValue(stanceModification.CurrentCamber);
            camberSlider.UpdateSlider(camberNormalized);
            
            float offsetNormalized = stanceModification.Offset.NormalizeValue(wheelToUse.transform.localPosition.x);
            offsetSlider.UpdateSlider(offsetNormalized);
            
            float tyreProfileNormalized = stanceModification.TyreProfile.NormalizeValue(stanceModification.WheelMesh.Tyre.transform.localScale.x);
            tyreProfileSlider.UpdateSlider(tyreProfileNormalized);
            
            float tyreWidthNormalized = stanceModification.TyreWidth.NormalizeValue(stanceModification.WheelMesh.Tyre.transform.localScale.z);
            tyreWidthSlider.UpdateSlider(tyreWidthNormalized);
            
            float rimDiameterNormalized = stanceModification.RimDiameter.NormalizeValue(stanceModification.WheelMesh.transform.localScale.y);
            rimDiameterSlider.UpdateSlider(rimDiameterNormalized);
            
            float rimWidthNormalized = stanceModification.RimWidth.NormalizeValue(stanceModification.WheelMesh.transform.localScale.x);
            rimWidthSlider.UpdateSlider(rimWidthNormalized);
        }

        public void OnSuspensionHeightSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

                float valueDenormalized = stanceModification.SuspensionHeight.DenormalizeValue(suspensionHeightSlider.Slider.value);
                stanceModification.ApplySuspensionHeight(valueDenormalized);
            }
        }
        
        public void OnCamberSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.Camber.DenormalizeValue(camberSlider.Slider.value);
                stanceModification.ApplyCamber(valueDenormalized);
            }
        }
        
        public void OnOffsetSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.Offset.DenormalizeValue(offsetSlider.Slider.value);
                stanceModification.ApplyOffset(valueDenormalized);
            }
        }
                
        public void OnTyreProfileSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.TyreProfile.DenormalizeValue(tyreProfileSlider.Slider.value);
                stanceModification.ApplyTyreProfile(valueDenormalized);
            }
        }
        
        public void OnTyreWidthSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.TyreWidth.DenormalizeValue(tyreWidthSlider.Slider.value);
                stanceModification.ApplyTyreWidth(valueDenormalized);
            }
        }
        
        public void OnRimDiameterSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.RimDiameter.DenormalizeValue(rimDiameterSlider.Slider.value);
                stanceModification.ApplyRimDiameter(valueDenormalized);
            }
        }
        
        public void OnRimWidthSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                
                float valueDenormalized = stanceModification.RimWidth.DenormalizeValue(rimWidthSlider.Slider.value);
                stanceModification.ApplyRimWidth(valueDenormalized);
            }
        }
        
    }
}
