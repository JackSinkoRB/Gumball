using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public enum ActionMapType
        {
            Car,
            General
        }

        public static InputAction PrimaryContact => Instance.GetOrCacheAction("PrimaryContact");
        public static InputAction PrimaryPosition => Instance.GetOrCacheAction("PrimaryPosition");

        public static ReadOnlyArray<Touch> ActiveTouches => Touch.activeTouches;
        
        public static VirtualInputActionFloat Steering { get; private set; }
        public static VirtualInputActionButton Accelerate { get; private set; }
        public static VirtualInputActionButton Decelerate { get; private set; }
        public static VirtualInputActionButton Handbrake { get; private set; }
        public static VirtualInputActionButton ShiftUp { get; private set; }
        public static VirtualInputActionButton ShiftDown { get; private set; }

        public static float SteeringInput => Steering?.Value ?? 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitialise()
        {
            try
            {
                CoroutineHelper.Instance.PerformAfterTrue(() => ExistsRuntime, () =>
                {
                    Steering = new VirtualInputActionFloat(Instance.GetOrCacheAction("Steering"));
                    Accelerate = new VirtualInputActionButton(Instance.GetOrCacheAction("Accelerate"));
                    Decelerate = new VirtualInputActionButton(Instance.GetOrCacheAction("Decelerate"));
                    Handbrake = new VirtualInputActionButton(Instance.GetOrCacheAction("Handbrake"));
                    ShiftUp = new VirtualInputActionButton(Instance.GetOrCacheAction("ShiftUp"));
                    ShiftDown = new VirtualInputActionButton(Instance.GetOrCacheAction("ShiftDown"));
                });
            }
            catch (NullReferenceException)
            {
                //CoroutineHelper might not exist in the scene
            }
        }
        
        [SerializeField] private PlayerInput playerInput;

        private readonly Dictionary<string, InputAction> actionsCached = new();
        private readonly Dictionary<ActionMapType, InputActionMap> actionsMapsCached = new();

        public PlayerInput PlayerInput => playerInput;
        
        protected override void Initialise()
        {
            base.Initialise();
            
            EnableActionMap(ActionMapType.General);
            EnhancedTouchSupport.Enable();
        }

        protected override void OnInstanceDisabled()
        {
            base.OnInstanceDisabled();
            
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            PinchInput.CheckForPinch();
        }

        private InputAction GetOrCacheAction(string action)
        {
            if (!actionsCached.ContainsKey(action))
                actionsCached[action] = playerInput.actions.FindAction(action);
        
            return actionsCached[action];
        }

        private InputActionMap GetActionMap(ActionMapType type)
        {
            if (!actionsMapsCached.ContainsKey(type))
            {
                //cache it
                actionsMapsCached[type] = playerInput.actions.FindActionMap(type.ToString());
            }

            return actionsMapsCached[type];
        }

        public void EnableActionMap(ActionMapType type, bool enable = true)
        {
            InputActionMap map = GetActionMap(type);
            if (enable)
                map.Enable();
            else map.Disable();
            
            GlobalLoggers.InputLogger.Log($"{(enable ? "Enabled" : "Disabled")} action map {type.ToString()}");
        }

    }
}
