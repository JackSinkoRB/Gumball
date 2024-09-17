using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        [Header("Cosmetic")]
        [SerializeField] private bool ignoreMainGraphic;
        [Space(5)]
        [SerializeField, Range(0,1)] private float pressedColorDarkeningPercent = 0.3f;
        [SerializeField] private float pressedTweenDuration = 0.15f;
        [Space(5)]
        [SerializeField, Range(0,1)] private float disabledAlpha = 0.5f;
        [SerializeField] private float disabledTweenDuration = 0.2f;
        [Space(5)]
        [SerializeField] private float transformsToMoveFadeTweenDuration = 0.15f;
        [Tooltip("This will move the listed transforms under the pointer when holding the button.")]
        [SerializeField] private RectTransform[] transformsToMoveUnderPointer;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isInteractable = true;
        [SerializeField, ReadOnly] private bool isPressingButton;
        [SerializeField, ReadOnly] private List<Graphic> targetGraphics = new();
        
        private bool isInitialised;
        private readonly Dictionary<Graphic, Color> initialColors = new();
        private Sequence pressedColorTween;
        private Sequence interactableColorTween;
        private Sequence transformsToMoveFadeTween;

        private RectTransform canvasRectTransform;
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

        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
            
            //disable the transforms to move until pressed
            foreach (RectTransform rectTransformToMove in transformsToMoveUnderPointer)
                rectTransformToMove.gameObject.SetActive(false);
        }

        private void Initialise()
        {
            isInitialised = true;

            FindTargetGraphics();

            canvasRectTransform = transform.GetComponentInAllParents<CanvasScaler>().transform as RectTransform;
        }

        private void Update()
        {
            CheckIfButtonIsPressed();
            if (IsPressingButton)
                OnHold();
        }

        public void SetInteractable(bool isInteractable)
        {
            if (this.isInteractable == isInteractable)
                return; //already set
                
            this.isInteractable = isInteractable;

            DoInteractableColorTween(isInteractable);
        }
        
        private void FindTargetGraphics()
        {
            targetGraphics = transform.GetComponentsInAllChildren<Graphic>();

            if (!ignoreMainGraphic)
            {
                Graphic ownGraphic = transform.GetComponent<Graphic>();
                if (ownGraphic != null)
                    targetGraphics.Add(ownGraphic);
            }

            foreach (Graphic graphic in targetGraphics)
                initialColors[graphic] = graphic.color;
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

            if (isPressed && !IsPressingButton && isInteractable)
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
            if (!IsPressingButton && canOnlyBePressedOnPointerDown && previousTouches.Contains(touch))
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

            DoPressColorTween(true);
            DoTransformsToMoveColorTween(true);
            
            onPress?.Invoke();
            onStartPressing?.Invoke();
            GlobalLoggers.InputLogger.Log($"Pressed {gameObject.name}");
        }

        private void OnRelease()
        {
            isPressingButton = false;

            DoPressColorTween(false);
            DoTransformsToMoveColorTween(false);

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

            MoveTransformsUnderPointer();

            onHoldPress?.Invoke();
        }

        private void OnDrag(Vector2 offset)
        {
            onDrag?.Invoke(offset);
        }
        
        private void DoTransformsToMoveColorTween(bool enabling)
        {
            transformsToMoveFadeTween?.Kill();
            transformsToMoveFadeTween = DOTween.Sequence();
            
            foreach (RectTransform rectTransformToMove in transformsToMoveUnderPointer)
            {
                if (enabling)
                    rectTransformToMove.GetOrAddComponent<CanvasGroup>().alpha = 0; //start at 0 whenever it's moved
                
                rectTransformToMove.gameObject.SetActive(true);
                transformsToMoveFadeTween.Join(rectTransformToMove.GetOrAddComponent<CanvasGroup>().DOFade(enabling ? 1 : 0, transformsToMoveFadeTweenDuration));
            }

            if (!enabling)
            {
                transformsToMoveFadeTween.OnComplete(() =>
                {
                    foreach (RectTransform rectTransformToMove in transformsToMoveUnderPointer)
                        rectTransformToMove.gameObject.SetActive(false);
                });
            }
        }

        private void MoveTransformsUnderPointer()
        {
            foreach (RectTransform rectTransformToMove in transformsToMoveUnderPointer)
            {
                //convert screen position to local position within the canvas
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, PrimaryContactInput.Position, null, out Vector2 localPoint);
                rectTransformToMove.localPosition = localPoint;
            }
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

        private void DoInteractableColorTween(bool isInteractable)
        {
            pressedColorTween?.Kill(); //take priority over press color tween
            interactableColorTween?.Kill();
            interactableColorTween = DOTween.Sequence();
            
            foreach (Graphic graphic in targetGraphics)
            {
                Color interactableColor = initialColors[graphic];
                Color disabledColor = initialColors[graphic].WithAlphaSetTo(disabledAlpha);
                
                interactableColorTween.Join(graphic.DOColor(isInteractable ? interactableColor : disabledColor, disabledTweenDuration));
            }
        }
        
        private void DoPressColorTween(bool isPressed)
        {
            if (!isInteractable)
                return; //interactable color tween takes priority
            
            interactableColorTween?.Kill();
            pressedColorTween?.Kill();
            pressedColorTween = DOTween.Sequence();
            
            foreach (Graphic graphic in targetGraphics)
            {
                Color pressedColor = new Color(graphic.color.r - pressedColorDarkeningPercent, graphic.color.g - pressedColorDarkeningPercent, graphic.color.b - pressedColorDarkeningPercent);
                Color depressedColor = initialColors[graphic];
                
                pressedColorTween.Join(graphic.DOColor(isPressed ? pressedColor : depressedColor, pressedTweenDuration));
            }
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
