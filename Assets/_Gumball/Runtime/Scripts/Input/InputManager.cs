using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public static InputAction Steering => Instance.GetOrCacheAction("Steering");
        public static InputAction Accelerate  => Instance.GetOrCacheAction("Accelerate");
        public static InputAction Decelerate => Instance.GetOrCacheAction("Decelerate");
        public static InputAction Handbrake => Instance.GetOrCacheAction("Handbrake");
        public static InputAction ShiftUp  => Instance.GetOrCacheAction("ShiftUp");
        public static InputAction ShiftDown  => Instance.GetOrCacheAction("ShiftDown");
        
        public static float SteeringInput => Steering.ReadValue<float>();

        [SerializeField] private InputActionAsset controls;

        private readonly Dictionary<string, InputAction> actionsCached = new();
        
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
