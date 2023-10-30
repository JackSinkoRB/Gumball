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
    public class ButtonEvents : MonoBehaviour
    {

        [SerializeField] private UnityEvent onStartPressing;
        [SerializeField] private UnityEvent onStopPressing;
        
        public bool IsPressingButton { get; private set; }

        private Button button => GetComponent<Button>();

        private void Update()
        {
            CheckIfButtonIsPressed();
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
        
        private void CheckIfButtonIsPressed()
        {
            bool isPressed = false;
            foreach (Touch touch in InputManager.ActiveTouches)
            {
                if (IsScreenPositionWithinGraphic(button.image, touch.screenPosition))
                {
                    isPressed = true;
                    break;
                }
            }

            if (isPressed && !IsPressingButton)
            {
                //just started pressing
                onStartPressing?.Invoke();
                GlobalLoggers.InputLogger.Log($"Pressed {gameObject.name}");
            }
            
            if (!isPressed && IsPressingButton)
            {
                //just stopped pressing
                onStopPressing?.Invoke();
                GlobalLoggers.InputLogger.Log($"Depressed {gameObject.name}");
            }
            
            IsPressingButton = isPressed;
        }
    }
}
