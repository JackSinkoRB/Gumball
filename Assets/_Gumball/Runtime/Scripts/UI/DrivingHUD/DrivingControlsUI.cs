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
            //listen for car change - or detect if there is currently a car
            WarehouseManager.Instance.onCurrentCarChanged += OnCarChanged;
            if (WarehouseManager.Instance.CurrentCar != null)
                OnCarChanged(WarehouseManager.Instance.CurrentCar);

            GearboxSetting.onSettingChanged += OnGearboxSettingChanged;
            
            //update with the current setting
            OnGearboxSettingChanged(GearboxSetting.Setting);
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChanged;
            GearboxSetting.onSettingChanged -= OnGearboxSettingChanged;

            if (InputManager.ExistsRuntime)
            {
                //disable the press overrides in case they were still pressed upon disabling
                InputManager.Instance.CarInput.Accelerate.SetPressedOverride(false);
                InputManager.Instance.CarInput.Brake.SetPressedOverride(false);
                InputManager.Instance.CarInput.Handbrake.SetPressedOverride(false);
                InputManager.Instance.CarInput.ShiftUp.SetPressedOverride(false);
                InputManager.Instance.CarInput.ShiftDown.SetPressedOverride(false);
                InputManager.Instance.CarInput.Steering.SetValueOverride(null);
            }
        }

        private void OnCarChanged(AICar newCar)
        {
            newCar.onGearChanged += OnGearChanged;
            OnGearChanged(-1, newCar.CurrentGear); //initialise the gear UI
        }

        private void OnGearboxSettingChanged(GearboxSetting.GearboxOption newValue)
        {
            gearUpButton.gameObject.SetActive(newValue == GearboxSetting.GearboxOption.MANUAL);
            gearDownButton.gameObject.SetActive(newValue == GearboxSetting.GearboxOption.MANUAL);
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
