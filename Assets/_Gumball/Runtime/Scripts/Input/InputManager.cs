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
        
        [SerializeField] private InputActionAsset controls;

        private readonly Dictionary<string, InputAction> actionsCached = new();

        protected override void Initialise()
        {
            base.Initialise();
            
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private InputAction GetOrCacheAction(string action)
        {
            if (!actionsCached.ContainsKey(action))
                actionsCached[action] = controls.FindAction(action);
        
            return actionsCached[action];
        }

        public void SetActionMap(ActionMapType actionMapType)
        {
            InputActionMap actionMap = controls.FindActionMap(actionMapType.ToString());
            actionMap.Enable();
        }
        
    }
}
