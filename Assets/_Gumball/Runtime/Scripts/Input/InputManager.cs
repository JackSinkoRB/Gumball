using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Gumball
{
    public class InputManager : Singleton<InputManager>
    {
        
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private PlayerInput playerInput;

        [Header("Driving")]
        [SerializeField] private DrivingControlLayoutManager drivingControlLayoutManager;
        
        [Header("Action maps")]
        [SerializeField] private GeneralInputManager generalInput;
        [SerializeField] private CarInputManager carInput;

        public InputActionAsset Controls => controls;
        public DrivingControlLayoutManager DrivingControlLayoutManager => drivingControlLayoutManager;
        public GeneralInputManager GeneralInput => generalInput;
        public CarInputManager CarInput => carInput;

        public static ReadOnlyArray<Touch> ActiveTouches => Touch.activeTouches;

        protected override void Initialise()
        {
            base.Initialise();
            
            EnhancedTouchSupport.Enable();
            GlobalLoggers.InputLogger.Log("Enhanced touch support has been force enabled.");
            
            ForceTouchSimulation();

            generalInput.Enable();
        }
        
        private void Update()
        {
            PinchInput.CheckForPinch();
        }
        
        private void ForceTouchSimulation()
        {
            TouchSimulation.Enable();

            DontDestroyOnLoad(TouchSimulation.instance.gameObject);

            //need to set the device as the current device after enabled
            InputDevice touchscreen = InputSystem.devices.First(device => device == Touchscreen.current);
            playerInput.SwitchCurrentControlScheme(touchscreen);
            
            GlobalLoggers.InputLogger.Log("Touch simulation has been force enabled.");
        }
        
    }
}
