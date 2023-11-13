using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagneticScrollUtils
{
    public class ScrollIcon : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        [Header("Components - Optional")]
        [SerializeField] private Image image;

        public Image ImageComponent => image;

        [field: SerializeField, ReadOnly] public RectTransform RectTransform { get; private set; }
        [field: SerializeField, ReadOnly] public Button Button { get; private set; }
        public Tween CurrentTween { get; private set; }

        [Header("Effects - Optional")]
        [Tooltip("The visual offset rect transform should be different to the 'ScrollIcon' object, as it just offsets the position from the ScrollIcon.")]
        public RectTransform visualOffsetEffectRectTransform;

        [SerializeField] private RectTransform rectTransformToScale;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private MagneticScroll scrollBelongsTo;

        [field: SerializeField, ReadOnly] public ScrollItem CurrentItem { get; private set; }
        [SerializeField, ReadOnly] private Vector2 defaultScaleSizeDelta;

        public MagneticScroll ScrollBelongsTo => scrollBelongsTo;

        /// <summary>
        /// Called when the icon is initialised in a new magnetic scroll.
        /// </summary>
        public void Initialise(MagneticScroll scrollBelongsTo)
        {
            this.scrollBelongsTo = scrollBelongsTo;

            InitialiseComponents();
            CheckForAffectingLayoutGroups();
            InitialiseAnchoring(scrollBelongsTo.ScrollDirection);
            AddClickListeners();
        }

        public void SetCurrentItem(ScrollItem item)
        {
            CurrentItem = item;
        }

        public void SetScaleInRespectToDefaultScale(Vector2 scale)
        {
            rectTransformToScale.localScale = new Vector2(scale.x * defaultScaleSizeDelta.x, scale.y * defaultScaleSizeDelta.y);
        }

        public void SetScaleInRespectToDefaultScaleForAxis(ScrollScaleEffect.ScaleAxis axis, float scale)
        {
            switch (axis)
            {
                case ScrollScaleEffect.ScaleAxis.Both:
                    SetScaleInRespectToDefaultScale(new Vector2(scale, scale));
                    break;
                case ScrollScaleEffect.ScaleAxis.X:
                    rectTransformToScale.localScale = new Vector2(scale * defaultScaleSizeDelta.x, rectTransformToScale.localScale.y);
                    break;
                case ScrollScaleEffect.ScaleAxis.Y:
                    rectTransformToScale.localScale = new Vector2(rectTransformToScale.localScale.x, scale * defaultScaleSizeDelta.y);
                    break;
            }

        }

        public void TweenToPos(Vector2 pos, float time)
        {
            CurrentTween?.Kill();
            CurrentTween = RectTransform.DOAnchorPos(pos, time).OnComplete(() =>
                CoroutineHelper.PerformAtEndOfFrame(() => CurrentTween = null)); //if ended before the frame, the final frame update won't apply effects
        }

        #region EVENTS TO SEND TO MAGNETIC SCROLL

        //make sure all click events are sent to the magnetic scroll

        public void OnPointerDown(PointerEventData eventData)
        {
            scrollBelongsTo.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            scrollBelongsTo.OnPointerUp(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            scrollBelongsTo.OnDrag(eventData);

            //if the player drags while clicking, make sure it doesn't call the onClick
            eventData.eligibleForClick = false;
        }

        #endregion

        /// <summary>
        /// An editor only check that provides warnings where the icon is affected by a layout group, which can cause issues with the positioning of the magnetic scroll.
        /// </summary>
        private void CheckForAffectingLayoutGroups()
        {
#if UNITY_EDITOR
            //loop each parent until hits magnetic scroll, and check if parent has layout group component. If so, disable it
            Transform transformToCheck = transform;
            while (true)
            {
                LayoutGroup layoutGroup = transformToCheck.GetComponent<LayoutGroup>();
                if (layoutGroup != null && layoutGroup.enabled && layoutGroup.gameObject.activeInHierarchy)
                {
                    Debug.LogError("There is a layout group (" + layoutGroup.gameObject.name + ") " +
                                   "affecting the ScrollIcon '" + gameObject.name + "'." +
                                   "Please disable it before runtime to avoid positioning issues with the magnetic scroll.");
                }

                if (transformToCheck.parent == null)
                {
                    break;
                }

                transformToCheck = transformToCheck.parent;
            }
#endif
        }

        /// <summary>
        /// Adjust the anchoring to a specific standard so the size calculations all match correctly.
        /// </summary>
        /// <remarks>This should be called when the scroll icon is initialised in a new magnetic scroll.</remarks>
        /// <param name="scrollDirection">The direction of the magnetic scroll is belongs to.</param>
        private void InitialiseAnchoring(MagneticScroll.Direction scrollDirection)
        {
            if (scrollDirection == MagneticScroll.Direction.HORIZONTAL)
            {
                //horizontal
                RectTransform.anchorMin = new Vector2(0, 0.5f);
                RectTransform.anchorMax = new Vector2(0, 0.5f);
                RectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                //vertical
                RectTransform.anchorMin = new Vector2(0.5f, 1f);
                RectTransform.anchorMax = new Vector2(0.5f, 1f);
                RectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        private void InitialiseComponents()
        {
            RectTransform = GetComponent<RectTransform>();

            if (rectTransformToScale == null)
            {
                rectTransformToScale = RectTransform;
            }

            Button = GetComponent<Button>();

            defaultScaleSizeDelta = rectTransformToScale.localScale;

            if (visualOffsetEffectRectTransform == RectTransform)
            {
                Debug.LogError("The visual offset effect rect should be different to the ScrollIcon object (" + gameObject.name + "). Set it as a child object (or unassign if the effect isn't used).");
            }
        }
        
        private void AddClickListeners()
        {
            if (Button == null)
            {
                if (scrollBelongsTo.SelectIconIfButtonClicked)
                {
                    //add the button component
                    AddButtonComponent();
                }
                else
                {
                    //no button component on icon
                    return;
                }
            }

            //reset the listener
            Button.onClick.RemoveAllListeners();

            //also add the click listener on the icon
            Button.onClick.AddListener(() => scrollBelongsTo.OnClickIcon(this));
        }
        
        /// <summary>
        /// Adds a button component to the gameobject, while updating the button reference.
        /// </summary>
        private void AddButtonComponent()
        {
            Button = gameObject.GetOrAddComponent<Button>();

            Button.enabled = true;

            //set the image as the target graphic, and make sure it can be clicked
            Button.targetGraphic = image;
            Button.targetGraphic.raycastTarget = true;
        }

    }
}