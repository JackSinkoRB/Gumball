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
        }

        public void OnSliderChanged()
        {
            foreach (WheelCollider wheelCollider in wheelsToModify)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

                //apply suspension height
                float suspensionHeightDenormalized = stanceModification.SuspensionHeight.DenormalizeValue(suspensionHeightSlider.value);
                stanceModification.ApplySuspensionHeight(suspensionHeightDenormalized);
                
                //apply camber
                float camberDenormalized = stanceModification.Camber.DenormalizeValue(camberSlider.value);
                stanceModification.ApplyCamber(camberDenormalized);
            }
            
            WarehouseManager.Instance.CurrentCar.UpdateWheelMeshes();
        }
        
    }
}
