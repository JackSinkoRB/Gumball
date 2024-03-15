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
            Drivetrain.onGearChanged += OnGearChanged;
            SettingsManager.Instance.onGearboxSettingChanged += OnGearboxSettingChanged;

            OnGearboxSettingChanged(SettingsManager.GearboxSetting);
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChanged;
            Drivetrain.onGearChanged -= OnGearChanged;
            SettingsManager.Instance.onGearboxSettingChanged -= OnGearboxSettingChanged;
        }

        private void OnCarChanged(CarManager newCar)
        {
            OnGearChanged(-1, newCar.drivetrain.Gear); //initialise the gear UI
        }

        private void OnGearboxSettingChanged(int newValue)
        {
            bool isAutomatic = newValue == 0;
            gearUpButton.gameObject.SetActive(!isAutomatic);
            gearDownButton.gameObject.SetActive(!isAutomatic);
        }
        
        private void OnGearChanged(int previousgear, int newgear)
        {
            Drivetrain drivetrain = WarehouseManager.Instance.CurrentCar.drivetrain;
            
            if (!drivetrain.automatic)
            {
                gearDownButton.interactable = drivetrain.Gear > 1;
                gearUpButton.interactable = drivetrain.Gear < drivetrain.gearRatios.Length - 1;
            }
        }

        public void OnPressAccelerateButton(bool isPressed)
        {
            InputManager.Accelerate.SetPressedOverride(isPressed);
        }

        public void OnPressBrakeButton(bool isPressed)
        {
            InputManager.Brake.SetPressedOverride(isPressed);
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
            InputManager.Handbrake.SetPressedOverride(isPressed);
        }

        public void OnPressGearDownButton(bool isPressed)
        {
            InputManager.ShiftDown.SetPressedOverride(isPressed);
        }

        public void OnPressGearUpButton(bool isPressed)
        {
            InputManager.ShiftUp.SetPressedOverride(isPressed);
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
            
            InputManager.Steering.SetValueOverride(steeringValue);
        }
        
    }
}
