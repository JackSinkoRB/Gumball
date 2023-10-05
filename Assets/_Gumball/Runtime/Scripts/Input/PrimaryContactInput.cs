using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gumball
{
    public static class PrimaryContactInput
    {

        public static event Action onPress;
        public static event Action onRelease;
        public static event Action onPerform;

        public static bool IsPressed { get; private set; }
        public static Vector2 Position { get; private set; }
        /// <summary>
        /// The position of the press when the primary contact was started.
        /// </summary>
        public static Vector2 PositionOnPress { get; private set; }

        /// <summary>
        /// The amount the primary position has moved since pressed.
        /// </summary>
        public static Vector2 OffsetSincePressed;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialise()
        {
            InputManager.PrimaryContact.started -= OnPressed;
            InputManager.PrimaryContact.started += OnPressed;

            InputManager.PrimaryContact.canceled -= OnReleased;
            InputManager.PrimaryContact.canceled += OnReleased;
            
            InputManager.PrimaryPosition.performed -= OnPerformed;
            InputManager.PrimaryPosition.performed += OnPerformed;
        }
        
        public static void OnPressed(InputAction.CallbackContext context)
        {
            IsPressed = true;
            PositionOnPress = InputManager.PrimaryPosition.ReadValue<Vector2>();
            Position = PositionOnPress;
            OffsetSincePressed = Vector2.zero;
            
            onPress?.Invoke();
        }

        public static void OnReleased(InputAction.CallbackContext context)
        {
            IsPressed = false;
            
            onRelease?.Invoke();
        }

        public static void OnPerformed(InputAction.CallbackContext context)
        {
            if (!IsPressed)
                return; //mouse input performs position despite being pressed

            Position = context.ReadValue<Vector2>();
            OffsetSincePressed = PositionOnPress - Position;
            
            onPerform?.Invoke();
        }

        public static bool IsSelectableUnderPointer()
        {
            var pointer = new PointerEventData(EventSystem.current) { position = Position };
        
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.GetComponent<Selectable>() != null);
                return true;
            }

            return false;
        }

    }
}
