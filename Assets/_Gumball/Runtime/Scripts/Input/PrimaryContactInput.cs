using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gumball
{
    public static class PrimaryContactInput
    {

        public static event Action onPress;
        public static event Action onDragStart;
        public static event Action<Vector2> onDrag;
        public static event Action onDragStop;
        public static event Action onRelease;
        public static event Action onPerform;

        public static bool IsPressed { get; private set; }
        public static bool IsDragging { get; private set; }
        
        public static Vector2 Position { get; private set; }
        /// <summary>
        /// The position of the press when the primary contact was started.
        /// </summary>
        public static Vector2 PositionOnPress { get; private set; }

        /// <summary>
        /// The amount the primary position has moved since pressed.
        /// </summary>
        public static Vector2 OffsetSincePressed;

        private static Vector2 lastKnownPosition;
        private static int selectablesUnderPointerLastCached = -1;
        private static readonly List<Selectable> selectablesUnderPointerCached = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialise()
        {
            CoroutineHelper.PerformAfterTrue(() => InputManager.ExistsRuntime, () =>
            {
                InputManager.PrimaryContact.started -= OnPressed;
                InputManager.PrimaryContact.started += OnPressed;

                InputManager.PrimaryContact.canceled -= OnReleased;
                InputManager.PrimaryContact.canceled += OnReleased;

                InputManager.PrimaryPosition.performed -= OnPerformed;
                InputManager.PrimaryPosition.performed += OnPerformed;
            });
        }
        
        public static void OnPressed(InputAction.CallbackContext context)
        {
            IsPressed = true;
            PositionOnPress = InputManager.PrimaryPosition.ReadValue<Vector2>();
            if (PositionOnPress.Approximately(Vector2.zero))
                return;
            
            Position = PositionOnPress;
            lastKnownPosition = Position;
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
            
            Vector2 offsetSinceLastFrame = Position - lastKnownPosition;
            lastKnownPosition = Position;
            if (offsetSinceLastFrame.sqrMagnitude > 0.01f)
            {
                if (!IsDragging)
                    OnStartDragging();

                OnDrag(offsetSinceLastFrame);
            } else if (IsDragging)
            {
                OnStopDragging();
            }
        }
        
        private static void OnStartDragging()
        {
            onDragStart?.Invoke();
        }

        private static void OnDrag(Vector2 offset)
        {
            onDrag?.Invoke(offset);
        }

        private static void OnStopDragging()
        {
            onDragStop?.Invoke();
        }

        public static bool IsSelectableUnderPointer()
        {
            return GetSelectablesUnderPointer().Count > 0;
        }

        public static bool IsSelectableUnderPointer(Selectable selectable)
        {
            foreach (Selectable selectableUnderPointer in GetSelectablesUnderPointer())
            {
                if (selectableUnderPointer == selectable)
                    return true;
            }

            return false;
        }

        private static List<Selectable> GetSelectablesUnderPointer()
        {
            //because input only updates once per frame, cache the results for the entire frame
            bool isCached = selectablesUnderPointerLastCached == Time.frameCount;
            if (!isCached)
            {
                selectablesUnderPointerCached.Clear();
                
                selectablesUnderPointerLastCached = Time.frameCount;
                PointerEventData pointer = new PointerEventData(EventSystem.current) { position = Position };
        
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                foreach (RaycastResult result in raycastResults)
                {
                    Selectable selectable = result.gameObject.GetComponent<Selectable>(); 
                    if (selectable != null)
                        selectablesUnderPointerCached.Add(selectable);
                }
                
            }
            
            return selectablesUnderPointerCached;
        }

    }
}
