using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DrivingControlsUI : MonoBehaviour
    {

        [SerializeField] private Button gearUpButton;
        [SerializeField] private Button gearDownButton;

        private bool isSteerLeftButtonPressed;
        private bool isSteerRightButtonPressed;

        private void OnEnable()
        {
            WarehouseManager.Instance.onCurrentCarChanged += OnCarChanged;
            //Drivetrain.onGearChanged += OnGearChanged;
            SettingsManager.Instance.onGearboxSettingChanged += OnGearboxSettingChanged;

            OnGearboxSettingChanged(SettingsManager.GearboxSetting);
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChanged;
            //Drivetrain.onGearChanged -= OnGearChanged;
            SettingsManager.Instance.onGearboxSettingChanged -= OnGearboxSettingChanged;
        }

        private void OnCarChanged(AICar newCar)
        {
            OnGearChanged(-1, newCar.CurrentGear); //initialise the gear UI
        }

        private void OnGearboxSettingChanged(int newValue)
        {
            bool isAutomatic = newValue == 0;
            gearUpButton.gameObject.SetActive(!isAutomatic);
            gearDownButton.gameObject.SetActive(!isAutomatic);
        }
        
        private void OnGearChanged(int previousgear, int newgear)
        {
            AICar currentCar = WarehouseManager.Instance.CurrentCar;
            
            if (!currentCar.IsAutomaticTransmission)
            {
                gearDownButton.interactable = currentCar.CurrentGear > 1;
                gearUpButton.interactable = currentCar.CurrentGear < currentCar.NumberOfGears - 1;
            }
        }

        public void OnPressAccelerateButton(bool isPressed)
        {
            InputManager.Instance.CarInput.Accelerate.SetPressedOverride(isPressed);
        }

        public void OnPressBrakeButton(bool isPressed)
        {
            InputManager.Instance.CarInput.Brake.SetPressedOverride(isPressed);
        }

        public void OnPressSteerLeftButton(bool isPressed)
        {
            isSteerLeftButtonPressed = isPressed;
            OnSteeringButtonUpdated();
        }

        public void OnPressSteerRightButton(bool isPressed)
        {
            isSteerRightButtonPressed = isPressed;
            OnSteeringButtonUpdated();
        }

        public void OnPressHandbrakeButton(bool isPressed)
        {
            InputManager.Instance.CarInput.Handbrake.SetPressedOverride(isPressed);
        }

        public void OnPressGearDownButton(bool isPressed)
        {
            InputManager.Instance.CarInput.ShiftDown.SetPressedOverride(isPressed);
        }

        public void OnPressGearUpButton(bool isPressed)
        {
            InputManager.Instance.CarInput.ShiftUp.SetPressedOverride(isPressed);
        }

        private void OnSteeringButtonUpdated()
        {
            float? steeringValue = null;
            if (isSteerLeftButtonPressed && isSteerRightButtonPressed)
                steeringValue = 0;
            else if (isSteerLeftButtonPressed)
                steeringValue = -1;
            else if (isSteerRightButtonPressed)
                steeringValue = 1;
            
            InputManager.Instance.CarInput.Steering.SetValueOverride(steeringValue);
        }
        
    }
}
