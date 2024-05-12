using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Gumball
{
        /// <summary>
    /// Listen for presses from all active touches.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class VirtualButton : MonoBehaviour
    {
        
        public event Action onPress;
        public event Action<Vector2> onDrag;
        public event Action onRelease;

        [SerializeField] private UnityEvent onStartPressing;
        [SerializeField] private UnityEvent onHoldPress;
        [SerializeField] private UnityEvent onStopPressing;

        [Header("Settings")]
        [Tooltip("Should it only be pressed when the pointer goes down, or can the pointer go down, and then drag over the button for it to be pressed?")]
        [SerializeField] private bool canOnlyBePressedOnPointerDown;
        [Tooltip("Should the press be cancelled if the pointer is no longer on top of the selectable?")]
        [SerializeField] private bool pointerMustBeOnRect = true;
        [Tooltip("Can the button be pressed if it is blocked by graphics on top of this one?")]
        [SerializeField] private bool canBePressedIfBlocked = true;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isPressingButton;
        
        private Vector2 lastKnownPosition;
        private ReadOnlyArray<Touch> previousTouches;
        private GraphicRaycaster graphicRaycasterCached;
        
        public bool IsPressingButton => isPressingButton;
        
        private Image image => GetComponent<Image>();

        private GraphicRaycaster graphicRaycaster
        {
            get
            {
                if (graphicRaycasterCached == null)
                    graphicRaycasterCached = transform.GetComponentInAllParents<GraphicRaycaster>();
                return graphicRaycasterCached;
            }
        }
        
        private void Update()
        {
            CheckIfButtonIsPressed();
            if (IsPressingButton)
                OnHold();
        }

        private void CheckIfButtonIsPressed()
        {
            bool isPressed = false;
            foreach (Touch touch in InputManager.ActiveTouches)
            {
                if (IsButtonPressedWithTouch(touch))
                {
                    isPressed = true;
                    break;
                }
            }

            previousTouches = InputManager.ActiveTouches;

            if (isPressed && !IsPressingButton)
                OnPress();

            if (pointerMustBeOnRect)
            {
                if (!isPressed && IsPressingButton)
                    OnRelease();
            }
            else
            {
                if (!PrimaryContactInput.IsPressed && IsPressingButton)
                    OnRelease();
            }
        }
        
        private bool IsButtonPressedWithTouch(Touch touch)
        {
            if (canOnlyBePressedOnPointerDown && previousTouches.Contains(touch))
                return false;

            if (!IsScreenPositionWithinGraphic(image, touch.screenPosition))
                return false;

            if (!canBePressedIfBlocked)
            {
                Graphic graphicUnderPointer = GraphicUtils.GetClickableGraphic(graphicRaycaster, touch.screenPosition);
                if (graphicUnderPointer != null && graphicUnderPointer != image)
                    return false;
            }

            return true;
        }

        private void OnPress()
        {
            isPressingButton = true;
            
            lastKnownPosition = PrimaryContactInput.Position;

            onPress?.Invoke();
            onStartPressing?.Invoke();
            GlobalLoggers.InputLogger.Log($"Pressed {gameObject.name}");
        }

        private void OnRelease()
        {
            isPressingButton = false;

            onRelease?.Invoke();
            onStopPressing?.Invoke();
            GlobalLoggers.InputLogger.Log($"Depressed {gameObject.name}");
        }

        private void OnHold()
        {
            Vector2 offset = PrimaryContactInput.Position - lastKnownPosition;
            lastKnownPosition = PrimaryContactInput.Position;
            
            if (offset.sqrMagnitude > 0.01f)
                OnDrag(offset);

            onHoldPress?.Invoke();
        }

        private void OnDrag(Vector2 offset)
        {
            onDrag?.Invoke(offset);
        }
        
        private bool IsScreenPositionWithinGraphic(Graphic graphic, Vector2 screenPosition)
        {
            Vector4 padding = graphic.raycastPadding;

            Vector3[] corners = new Vector3[4];
            graphic.rectTransform.GetWorldCorners(corners);

            Rect rectWithPadding = new Rect(
                corners[0].x + padding.x,
                corners[0].y + padding.y,
                corners[2].x - corners[0].x - padding.x - padding.z,
                corners[2].y - corners[0].y - padding.y - padding.w
            );
#if UNITY_EDITOR
            rectToDraw = rectWithPadding;
#endif            
            
            return rectWithPadding.Contains(screenPosition);
        }

#if UNITY_EDITOR
        private Rect rectToDraw;
        private void OnDrawGizmos()
        {
            Handles.color = Color.red;
            Handles.DrawWireCube(rectToDraw.center, new Vector3(rectToDraw.width, rectToDraw.height, 0));
        }
#endif

    }
}
