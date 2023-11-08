using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Gumball
{
    /// <summary>
    /// Listen for presses from all active touches.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonEvents : MonoBehaviour
    {

        public event Action<Vector2> onPressMove;
        
        [SerializeField] private UnityEvent onStartPressing;
        [SerializeField] private UnityEvent onHoldPress;
        [SerializeField] private UnityEvent onStopPressing;

        [Header("Settings")]
        [Tooltip("Should the press be cancelled if the pointer is no longer on top of the selectable?")]
        [SerializeField] private bool pointerMustBeOnRect = true;

        [Header("Debugging")]
        [SerializeField] private bool isPressingButton;
        
        public Button Button => GetComponent<Button>();
        public bool IsPressingButton => isPressingButton;

        private Vector2 lastKnownPosition;

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
                if (IsScreenPositionWithinGraphic(Button.image, touch.screenPosition))
                {
                    isPressed = true;
                    break;
                }
            }

            if (isPressed && !IsPressingButton)
                OnPress();

            if (pointerMustBeOnRect && !isPressed && IsPressingButton)
                OnRelease();

            if (!pointerMustBeOnRect && !PrimaryContactInput.IsPressed)
                OnRelease();
        }

        private void OnPress()
        {
            isPressingButton = true;
            
            lastKnownPosition = PrimaryContactInput.Position;
            
            onStartPressing?.Invoke();
            GlobalLoggers.InputLogger.Log($"Pressed {gameObject.name}");
        }

        private void OnRelease()
        {
            isPressingButton = false;
            
            //just stopped pressing
            onStopPressing?.Invoke();
            GlobalLoggers.InputLogger.Log($"Depressed {gameObject.name}");
        }

        private void OnHold()
        {
            Vector2 offset = PrimaryContactInput.Position - lastKnownPosition;
            lastKnownPosition = PrimaryContactInput.Position;
            
            if (offset.sqrMagnitude > 0.01f)
                OnMove(offset);

            onHoldPress?.Invoke();
        }

        private void OnMove(Vector2 offset)
        {
            onPressMove?.Invoke(offset);
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
