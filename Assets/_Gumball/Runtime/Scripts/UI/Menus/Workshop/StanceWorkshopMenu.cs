using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class StanceWorkshopMenu : WorkshopSubMenu
    {

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
