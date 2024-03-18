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
        /// <summary>
        /// If there has been more movement than the drag threshold since the last frame.
        /// </summary>
        public static bool IsDragging { get; private set; }
        /// <summary>
        /// If there has been more movement than the drag threshold since pressing.
        /// </summary>
        public static bool HasDraggedSincePressing => !OffsetSincePressedNormalised.Approximately(Vector2.zero, DragThreshold);

        public static Vector2 Position { get; private set; }
        /// <summary>
        /// The position of the press when the primary contact was started.
        /// </summary>
        public static Vector2 PositionOnPress { get; private set; }

        /// <summary>
        /// The amount the primary position has moved since pressed.
        /// <remarks>This doesn't use screen position, so the value won't be equal among devices (eg. it will be greater if the screen size is bigger).</remarks>
        /// </summary>
        public static Vector2 OffsetSincePressed { get; private set; }
        /// <summary>
        /// The amount the primary position has moved since pressed (normalised with the screen size).
        /// </summary>
        public static Vector2 OffsetSincePressedNormalised { get; private set; }
        /// <summary>
        /// The amount the primary position has moved since the last frame.
        /// </summary>
        public static Vector2 OffsetSinceLastFrame { get; private set; }
        /// <summary>
        /// The amount of time passed (in seconds) since the pointer was last pressed.
        /// </summary>
        public static float TimeSincePressed { get; private set; }

        private static Vector2 lastKnownPositionOnPerformed;
        private static int graphicsUnderPointerLastCached = -1;
        private static readonly List<Graphic> clickablesUnderPointerCached = new();
        private static Vector2 lastKnownPosition;
        private static readonly RaycastHit[] collidersUnderPointer = new RaycastHit[20];

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
                InputManager.Instance.GeneralInput.PrimaryContact.started -= OnPressed;
                InputManager.Instance.GeneralInput.PrimaryContact.started += OnPressed;

                InputManager.Instance.GeneralInput.PrimaryContact.canceled -= OnReleased;
                InputManager.Instance.GeneralInput.PrimaryContact.canceled += OnReleased;

                InputManager.Instance.GeneralInput.PrimaryPosition.performed -= OnPerformed;
                InputManager.Instance.GeneralInput.PrimaryPosition.performed += OnPerformed;

                CoroutineHelper.onUnityUpdate -= Update;
                CoroutineHelper.onUnityUpdate += Update;
            });
        }
        
        private static void Update()
        {
            UpdateOffsetSinceLastFrame();
            TimeSincePressed += Time.deltaTime;
        }
        
        public static void OnPressed(InputAction.CallbackContext context)
        {
            IsPressed = true;
            PositionOnPress = InputManager.Instance.GeneralInput.PrimaryPosition.ReadValue<Vector2>();
            Position = PositionOnPress;
            lastKnownPositionOnPerformed = Position;
            OffsetSincePressed = Vector2.zero;
            OffsetSincePressedNormalised = Vector2.zero;
            OffsetSinceLastFrame = Vector2.zero;
            TimeSincePressed = 0;
            
            onPress?.Invoke();
        }

        public static void OnReleased(InputAction.CallbackContext context)
        {
            IsPressed = false;
            
            onRelease?.Invoke();
        }

        /// <summary>
        /// The amount of normalised offset required for the pointer to be considered to have been 'dragged'.
        /// </summary>
        public const float DragThreshold = 0.001f;

        /// <summary>
        /// The amount of normalised offset allowed for the pointer to be considered to have been 'pressed'.
        /// </summary>
        public const float PressedThreshold = 0.01f;
        
        public static void OnPerformed(InputAction.CallbackContext context)
        {
            if (!IsPressed)
                return; //mouse input performs position despite being pressed

            Position = context.ReadValue<Vector2>();
            OffsetSincePressed = PositionOnPress - Position;
            OffsetSincePressedNormalised = GetNormalisedScreenPosition(OffsetSincePressed);
            onPerform?.Invoke();

            bool isFirstContact = lastKnownPositionOnPerformed == Vector2.zero;
            Vector2 offsetSinceLastFrame = Position - lastKnownPositionOnPerformed;
            lastKnownPositionOnPerformed = Position;
            
            if (!isFirstContact) //don't detect drag if it's the first frame
                CheckForDrag(offsetSinceLastFrame);
        }

        public static bool IsColliderUnderPointer(Collider collider, float maxDistance = Mathf.Infinity, LayerMask layerMask = default)
        {
            //raycast from the pointer position into the world
            Ray ray = Camera.main.ScreenPointToRay(Position);

            //max raycast distance from the camera to the middle of the car, so it doesn't detect decals on the other side
            int raycastHits = Physics.RaycastNonAlloc(ray, collidersUnderPointer, maxDistance, layerMask);

            for (int count = 0; count < raycastHits; count++)
            {
                RaycastHit hit = collidersUnderPointer[count];
                if (hit.collider == collider)
                    return true;
            }

            return false;
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
        
        private static void CheckForDrag(Vector2 offsetSinceLastFrame)
        {
            Vector2 offsetSinceLastFrameNormalised = GetNormalisedScreenPosition(offsetSinceLastFrame);
            if (!offsetSinceLastFrameNormalised.Approximately(Vector2.zero, DragThreshold))
            {
                if (!IsDragging)
                    OnStartDragging();

                OnDrag(offsetSinceLastFrameNormalised);
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
        
        public static List<Graphic> GetClickableGraphicsUnderPointer()
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
