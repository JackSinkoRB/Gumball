using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ButtonsDrivingControlLayout : DrivingControlLayout
    {
        
        private bool isSteerLeftButtonPressed;
        private bool isSteerRightButtonPressed;

        private void OnDisable()
        {
            if (InputManager.ExistsRuntime)
            {
                //disable the press overrides in case they were still pressed upon disabling
                InputManager.Instance.CarInput.Accelerate.SetPressedOverride(false);
                InputManager.Instance.CarInput.Brake.SetPressedOverride(false);
                InputManager.Instance.CarInput.Handbrake.SetPressedOverride(false);
                InputManager.Instance.CarInput.Steering.SetValueOverride(null);
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
