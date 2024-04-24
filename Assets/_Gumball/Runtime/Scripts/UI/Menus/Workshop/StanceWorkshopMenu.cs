using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StanceWorkshopMenu : WorkshopSubMenu
    {

        //car has a 'StanceModification' class on the wheels object
        // - has the min/max values and default values
        
        //convert slider values to real values
        //slider values are saved to file
        // - they are converted whenever it needs to update
        
        //apply each slider value to the car
        // - call on slider change
        // - call when loading car
        
        [SerializeField] private Slider suspensionHeightSlider;

        private void OnEnable()
        {
            UpdateSliderValues();
        }

        /// <summary>
        /// Updates the slider values with the car's current values.
        /// </summary>
        private void UpdateSliderValues()
        {
            //TODO: handle if using front, rear, or all wheels
            foreach (WheelCollider wheelCollider in WarehouseManager.Instance.CurrentCar.AllWheelColliders)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

                float valueNormalized = stanceModification.SuspensionHeight.NormalizeValue(wheelCollider.suspensionDistance);
                suspensionHeightSlider.SetValueWithoutNotify(valueNormalized);

                break; //just use the first wheel found
            }
        }

        public void OnSuspensionHeightSliderChanged()
        {
            //TODO: handle if using front, rear, or all wheels
            foreach (WheelCollider wheelCollider in WarehouseManager.Instance.CurrentCar.AllWheelColliders)
            {
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

                float denormalizedValue = stanceModification.SuspensionHeight.DenormalizeValue(suspensionHeightSlider.value);
                stanceModification.ApplySuspensionHeight(denormalizedValue);
            }
            
            WarehouseManager.Instance.CurrentCar.UpdateWheelMeshes();
        }
    }
}
