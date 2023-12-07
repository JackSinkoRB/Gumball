using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static Vector2 OffsetSincePressed { get; private set; }
        /// <summary>
        /// The amount the primary position has moved since the last frame.
        /// </summary>
        public static Vector2 OffsetSinceLastFrame { get; private set; }

        private static Vector2 lastKnownPositionOnPerformed;
        private static int graphicsUnderPointerLastCached = -1;
        private static readonly List<Graphic> clickablesUnderPointerCached = new();
        private static Vector2 lastKnownPosition;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitialisePreSceneLoad()
        {
            onPress = null;
            onDragStart = null;
            onDrag = null;
            onDragStop = null;
            onRelease = null;
            onPerform = null;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitialisePostSceneLoad()
        {
            CoroutineHelper.PerformAfterTrue(() => InputManager.ExistsRuntime, () =>
            {
                InputManager.PrimaryContact.started -= OnPressed;
                InputManager.PrimaryContact.started += OnPressed;

                InputManager.PrimaryContact.canceled -= OnReleased;
                InputManager.PrimaryContact.canceled += OnReleased;

                InputManager.PrimaryPosition.performed -= OnPerformed;
                InputManager.PrimaryPosition.performed += OnPerformed;

                CoroutineHelper.onUnityUpdate -= Update;
                CoroutineHelper.onUnityUpdate += Update;
            });
        }
        
        private static void Update()
        {
            UpdateOffsetSinceLastFrame();
        }
        
        public static void OnPressed(InputAction.CallbackContext context)
        {
            IsPressed = true;
            PositionOnPress = InputManager.PrimaryPosition.ReadValue<Vector2>();
            Position = PositionOnPress;
            lastKnownPositionOnPerformed = Position;
            OffsetSincePressed = Vector2.zero;
            OffsetSinceLastFrame = Vector2.zero;
            
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

            Vector2 offsetSinceLastFrame = Position - lastKnownPositionOnPerformed;
            lastKnownPositionOnPerformed = Position;
            
            if (!offsetSinceLastFrame.Approximately(Vector2.zero))
            {
                if (!IsDragging)
                    OnStartDragging();

                Vector2 offsetNormalised = GetNormalisedScreenPosition(offsetSinceLastFrame);
                OnDrag(offsetNormalised);
            } else if (IsDragging)
            {
                OnStopDragging();
            }
        }

        /// <summary>
        /// Is a raycastable graphic under the pointer?
        /// </summary>
        /// <returns></returns>
        public static bool IsGraphicUnderPointer(Graphic[] exclusions = null)
        {
            foreach (Graphic graphic in GetClickableGraphicsUnderPointer())
            {
                bool isExcluded = exclusions != null && exclusions.Contains(graphic);
                if (!isExcluded)
                    return true;
            }

            return false;
        }

        public static bool IsGraphicUnderPointer(Graphic graphic)
        {
            foreach (Graphic graphicUnderPointer in GetClickableGraphicsUnderPointer())
            {
                if (graphicUnderPointer == graphic)
                    return true;
            }

            return false;
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
        
        private static List<Graphic> GetClickableGraphicsUnderPointer()
        {
            //because input only updates once per frame, cache the results for the entire frame
            bool isCached = graphicsUnderPointerLastCached == Time.frameCount;
            if (!isCached)
            {
                clickablesUnderPointerCached.Clear();
                
                graphicsUnderPointerLastCached = Time.frameCount;
                PointerEventData pointer = new PointerEventData(EventSystem.current) { position = Position };
        
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, raycastResults);

                foreach (RaycastResult result in raycastResults)
                {
                    Graphic graphic = result.gameObject.GetComponent<Graphic>();
                    if (graphic == null)
                        continue;
                    
                    clickablesUnderPointerCached.Add(graphic);
                }
                
            }
            
            return clickablesUnderPointerCached;
        }
        
        private static Vector2 GetNormalisedScreenPosition(Vector2 screenPosition)
        {
            return new Vector2(screenPosition.x / Screen.width, screenPosition.y / Screen.height);
        }

        private static void UpdateOffsetSinceLastFrame()
        {
            Vector2 offset = Position - lastKnownPosition;
            OffsetSinceLastFrame = GetNormalisedScreenPosition(offset);
            lastKnownPosition = Position;
        }

    }
}
