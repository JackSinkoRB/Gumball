using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Gumball
{
    public class ButtonsDrivingControlLayout : DrivingControlLayout
    {

        private enum SteerInputType
        {
            BUTTONS,
            TILT,
            SLIDE
        }
        
        /// <summary>
        /// The maximum tilt required for full steering angle (from 0 to 1 - 0 being no tilt and 1 being device is rotated 90 degrees).
        /// </summary>
        private const float maxTilt = 0.15f;
        //TODO: make this a setting in settings menu (lerp between a min max value)
        private const float tiltMultiplier = 0.3f;
        
        /// <summary>
        /// The amount of the screen to drag across for full steering angle with the slide input type.
        /// </summary>
        private const float slideScreenPercentForFullSteering = 0.2f;

        [SerializeField] private bool autoAccelerate;
        [SerializeField] private SteerInputType steerInputType;

        private bool isSteerLeftButtonPressed;
        private bool isSteerRightButtonPressed;
        private Vector2 horizontalInputChangeSincePress;

        protected override void OnActivate()
        {
            base.OnActivate();
            
            if (autoAccelerate)
                InputManager.Instance.CarInput.Accelerate.SetPressedOverride(true);

            if (steerInputType == SteerInputType.TILT)
                SensorManager.AddListener(this);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            
            if (InputManager.ExistsRuntime)
            {
                //disable the press overrides in case they were still pressed upon disabling
                InputManager.Instance.CarInput.Accelerate.SetPressedOverride(false);
                InputManager.Instance.CarInput.Brake.SetPressedOverride(false);
                InputManager.Instance.CarInput.Handbrake.SetPressedOverride(false);
                InputManager.Instance.CarInput.Steering.SetValueOverride(null);
            }
            
            if (steerInputType == SteerInputType.TILT)
                SensorManager.RemoveListener(this);
        }

        private void OnDisable()
        {
            if (layoutManager.CurrentLayout == this)
                SetInactive();
        }

        private void Update()
        {
            if (steerInputType == SteerInputType.TILT)
                SetSteeringFromTilt();
            
            if (steerInputType == SteerInputType.SLIDE)
                SetSteeringFromHorizontalInput();
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
        
        private void SetSteeringFromTilt()
        {
            //all racing games just take the X acceleration - meaning if phone is flat, it won't register any tilt 
            Vector3 acceleration = Accelerometer.current.acceleration.value;
            float tilt = acceleration.x * tiltMultiplier;
            
            float steeringValue = Mathf.Clamp(tilt / maxTilt, -1, 1);
            
            InputManager.Instance.CarInput.Steering.SetValueOverride(steeringValue);
        }

        private void SetSteeringFromHorizontalInput()
        {
            if (!PrimaryContactInput.IsPressed)
                //reset the offset
                horizontalInputChangeSincePress = Vector2.zero;

            foreach (Touch touch in InputManager.ActiveTouches)
                horizontalInputChangeSincePress += touch.delta;
         
            float horizontalPercent = GraphicUtils.GetNormalisedScreenPosition(horizontalInputChangeSincePress).x;
            float steeringValue = Mathf.Clamp(horizontalPercent / slideScreenPercentForFullSteering, -1, 1);
            
            InputManager.Instance.CarInput.Steering.SetValueOverride(steeringValue);
        }
        
    }
}
