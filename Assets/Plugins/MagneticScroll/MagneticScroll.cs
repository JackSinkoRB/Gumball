using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MagneticScrollUtils
{
    /// <summary>
    /// A wrappable scroll UI element.
    /// 'Icon' refers to the physical ScrollIcon element that wraps.
    /// 'Item' represents the item data that is populated into an icon.
    /// </summary>
    //require a canvas so that when the scroll is updated, it's not updating the entire UI.
    //require a graphic raycaster for detecting input.
    //require an image component so that it has something to click and adjust the raycast padding without inheriting from Graphic.
    [RequireComponent(typeof(Image), typeof(Canvas), typeof(GraphicRaycaster))]
    public class MagneticScroll : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        public enum Direction
        {
            HORIZONTAL,
            VERTICAL
        }

        #region STATIC ACCESS

        public static bool IsUsingScroll => ScrollUsing != null;
        public static MagneticScroll ScrollUsing = null;

        #endregion


        #region EVENTS

        /// <summary>
        /// Called when icons are moved.
        /// </summary>
        public event Action<Vector2> OnMoveAllItems;

        #endregion


        #region INSPECTOR SETTINGS & DEBUGGING

        [Header("Required")]
        
        [InitializationField, SerializeField] private Direction direction;

        [Space(5)]
        [InitializationField, SerializeField] private bool populatesAtRuntime;

        //if populated at runtime:
        [FormerlySerializedAs("scrollItemPrefab")]
        [ConditionalField(nameof(populatesAtRuntime)), InitializationField, SerializeField]
        private ScrollIcon scrollIconPrefab;

        //if not populated at runtime:
        [ConditionalField(nameof(populatesAtRuntime), true), InitializationField, SerializeField]
        protected CollectionWrapper<PreDefinedScrollItem> preDefinedItems = new();

        [Header("Sizing")]
        
        [Tooltip("The number of icons to instantiate in the magnetic scroll.\n\nNote: this should be as minimal as possible to reduce the number of objects instantiated." +
                 "\n\nThis is an odd number because there should always be an icon in the middle, with an even amount on each side.")]
        [FormerlySerializedAs("iconsToPopulate")]
        [ConditionalField(nameof(populatesAtRuntime)), InitializationField, SerializeField, RangeWithStep(1, 11, 2, 1)]
        private int numberOfIcons = 3;

        [InitializationField, SerializeField]
        private float spacingBetweenIcons = 20;

        [Tooltip("Extend or reduce the clickable area of the magnetic scroll, in relation to the icon sizes.")]
        [InitializationField, SerializeField]
        private RectOffset clickableAreaPadding;

        [Header("Settings")]
        
        [Tooltip("Toggle for the 'magnetic' feature of the scroll. If enabled, the closest icon will snap to the magnet when there is no user input.")]
        [SerializeField]
        private bool useMagnetSnapping = true;

        [FormerlySerializedAs("infiniteScroll")]
        [ConditionalField(nameof(populatesAtRuntime)), SerializeField]
        private bool useInfiniteScroll;

        [Tooltip("Does infinite scroll get disabled if there aren't enough items to fill all the icons?")]
        [ConditionalField(nameof(useInfiniteScroll)), SerializeField]
        private bool disableInfiniteScrollIfNotEnoughItems = true;

        [FormerlySerializedAs("selectIconWhenClicked")]
        [Tooltip("When an icon is clicked, should it snap to the magnet?")]
        [InitializationField, SerializeField] private bool selectIconIfButtonClicked;

        [Header("Input")]

        [SerializeField] private float scrollSpeed = 1;

        [Tooltip("The elasticity when infinite scroll is disabled and the user is trying to drag past the first or last icon")]
        [ConditionalField(nameof(disableInfiniteScrollIfNotEnoughItems)), SerializeField]
        private float elasticity = 5;

        [SerializeField] private float snapTime = 0.15f;
        [SerializeField] private float decelerationTime = 0.15f;

        [Header("Events")]
        
        [SerializeField] private UnityEvent onSelectIcon;

        [Header("Effects")]
        
        [SerializeField] private ScrollEffect[] effects;

        [Space(5)]
        [Tooltip("Show an outline image around the selected icon? Note: only supported when all icons are the same size")]
        [SerializeField] private bool useOutlineEffect;

        [ConditionalField(nameof(useOutlineEffect)), SerializeField] private Image outlineEffectImage;
        [ConditionalField(nameof(useOutlineEffect)), SerializeField] private float outlineEffectMinScale = 0.5f;
        [ConditionalField(nameof(useOutlineEffect)), SerializeField] private float outlineEffectMaxScale = 1f;

        [Header("Debugging")]
        [SerializeField] private bool isLoggingEnabled;

        [ReadOnly, SerializeField] protected bool initialised;
        [ReadOnly, SerializeField] protected List<ScrollIcon> icons = new();
        [ReadOnly, SerializeField] protected List<ScrollItem> items = new();

        [Space(5)]
        [Tooltip("The distance between the first and second icon." +
                 "Since MagneticScroll currently only supports icons of the same size, this will be the same distance for all icons.")]
        [ReadOnly, SerializeField] protected Vector3 distanceBetweenIcons;

        [ReadOnly, SerializeField] protected Vector2 scrollMaxPos;
        [ReadOnly, SerializeField] protected Vector2 scrollMinPos;

        [Space(5)]
        [ReadOnly, SerializeField] protected bool isScrolling;

        [ReadOnly, SerializeField] protected bool isDecelerating;

        [Space(5)]
        [Tooltip("The total movement offset since moving.")]
        [SerializeField, ReadOnly] protected Vector2 movementOffset;

        [ReadOnly, SerializeField] protected float timeOnPointerUp;
        [ReadOnly, SerializeField] protected Vector2 lastKnownDelta;
        [ReadOnly, SerializeField] protected Vector2 movement;

        [Space(5)]
        //because the icons can wrap, it is not always the first and last elements in the icons array
        [ReadOnly, SerializeField] protected int firstIconIndex;
        [ReadOnly, SerializeField] protected int lastIconIndex;

        [Space(5)]
        [ReadOnly, SerializeField] protected int firstItemIndexShowing;
        [ReadOnly, SerializeField] protected int lastItemIndexShowing;

        [Space(5)]
        [ReadOnly, SerializeField] protected ScrollIcon iconSnappingToMiddle;

        [Space(5)]
        [Tooltip("If the number of items in the scroll is less than the number of icons, the infinite scroll is disabled.")]
        [ReadOnly, SerializeField, ConditionalField(nameof(useInfiniteScroll))] protected bool isInfiniteScrollEnabled = true;

        [ReadOnly, SerializeField, ConditionalField(nameof(isInfiniteScrollEnabled), true)] protected int iconsInUse;
        [ReadOnly, SerializeField] private List<Vector2> initialIconPositions = new();

        [Tooltip("Icons that have been disabled due to extending past the max item index.")]
        [ReadOnly, SerializeField] private List<ScrollIcon> extendedIcons = new();

        [Tooltip("The amount of distance the magnet is away from the first or last icon.")]
        [ReadOnly, SerializeField] private float currentElasticityModifier;

        [ReadOnly, SerializeField] private int lastSelectedItemIndex;
        [ReadOnly, SerializeField] private int lastSelectedIconIndex;
        [ReadOnly, SerializeField] private int closestIconToMagnetIndex = -1;

        #endregion


        #region PRIVATE ACCESS

        private RectTransform rectTransform;
        private Vector2 lastKnownPositionOfFirstIcon;
        private Coroutine snapToItemOffscreenCoroutine;
        private int selectedItemWhenPointerDown = -1;
        private bool clickedToSelect;
        private bool interactable = true;

        private enum SnapToItemDirection
        {
            FORWARD,
            BACKWARD
        }

        #endregion


        #region PROPERTIES

        public Direction ScrollDirection
        {
            get => direction;
            set => direction = value;
        }

        public bool PopulatesAtRuntime => populatesAtRuntime;
        public int NumberOfIcons => populatesAtRuntime ? numberOfIcons : PreDefinedItems.Count;
        public ReadOnlyCollection<ScrollItem> Items => new(items);
        public float SpacingBetweenIcons => spacingBetweenIcons;
        public RectOffset ClickableAreaPadding => clickableAreaPadding ?? new RectOffset(); //check for null in case inspector hasn't been updated
        public bool SelectIconIfButtonClicked => selectIconIfButtonClicked;

        public int FirstIconIndex => firstIconIndex;
        public int LastIconIndex => lastIconIndex;
        public int FirstItemIndexShowing => firstItemIndexShowing;
        public int LastItemIndexShowing => lastItemIndexShowing;
        public List<ScrollIcon> Icons => icons;

        /// <summary>
        /// The index of the last selected item. Useful for recovering the last selected item when repopulating at runtime.
        /// <remarks>If the item order has changed, this value won't match the new positions.</remarks>
        /// </summary>
        public int LastSelectedItemIndex => lastSelectedItemIndex;
        public int LastSelectedIconIndex => lastSelectedIconIndex;
        
        public int ClosestIconToMagnetIndex => closestIconToMagnetIndex;

        /// <summary>
        /// Get the closest icon to the magnet. This will return null if there's been no input to calculate the closest icon.
        /// </summary>
        public ScrollIcon ClosestIconToMagnet =>
            closestIconToMagnetIndex == -1 || icons == null || icons.Count == 0 ? null : icons[closestIconToMagnetIndex];

        public bool IsHorizontal => direction == Direction.HORIZONTAL;
        public bool IsVertical => direction == Direction.VERTICAL;
        public bool IsMoving => isMoving;

        /// <summary>
        /// The predefined items being used when NOT populating at runtime.
        /// </summary>
        public ReadOnlyCollection<PreDefinedScrollItem> PreDefinedItems => preDefinedItems.Value == null
            ? new ReadOnlyCollection<PreDefinedScrollItem>(new List<PreDefinedScrollItem>())
            : new ReadOnlyCollection<PreDefinedScrollItem>(preDefinedItems.Value);

        /// <summary>
        /// The scroll icon prefab used when populating at runtime.
        /// </summary>
        public ScrollIcon ScrollIconPrefab => scrollIconPrefab;

        public bool CanInfiniteScroll => useInfiniteScroll && isInfiniteScrollEnabled;

        private bool isTweening => icons.Count > 0 && icons[0].CurrentTween != null; //check if the first item is tweening
        private bool isMoving => isScrolling || isDecelerating || isTweening;

        /// <summary>
        /// The magnet position in terms of the MagneticScroll rect transform.
        /// <remarks>This is now just the center of the scroll.</remarks>
        /// </summary>
        private Vector2 magnetPosition => new(IsHorizontal ? rectTransform.rect.width / 2 : 0, IsVertical ? -rectTransform.rect.height / 2 : 0);

        #endregion


        #region UNITY METHODS

        private void Awake()
        {
            if (!initialised)
            {
                Initialise();
            }
        }

        private void OnDisable()
        {
            //make sure scrolling is stopped
            OnStopScrolling();
        }

        private void Update()
        {
            CheckToDecelerate();

            if (isMoving)
            {
                CheckForTweenToCallMoveEvent();
            }
        }
        
        private void LateUpdate()
        {
            if (isMoving)
            {
                PostMovement();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            OnStartScrolling();

            clickedToSelect = false;
            selectedItemWhenPointerDown = lastSelectedItemIndex;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;
            OnStopScrolling();

            if (lastSelectedItemIndex != selectedItemWhenPointerDown)
                items[lastSelectedItemIndex].OnSelectComplete();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!interactable) return;
            if (ScrollUsing != this)
            {
                return;
            }

            //kill the snap-to-item coroutine as we have new input
            if (snapToItemOffscreenCoroutine != null)
            {
                StopCoroutine(snapToItemOffscreenCoroutine);
            }

            lastKnownDelta = eventData.delta;
            MoveAllItems(lastKnownDelta);

            if (isLoggingEnabled) Debug.Log("Dragged " + gameObject.name + ": " + movement);
        }

        #endregion


        #region PUBLIC METHODS

        public void SnapToNextItem()
        {
            int desiredIconIndex = lastSelectedIconIndex + 1;
            if (desiredIconIndex == icons.Count)
            {
                desiredIconIndex = 0; //wrap
            }

            int desiredItemIndex = lastSelectedItemIndex + 1;
            if (!CanInfiniteScroll && desiredItemIndex >= items.Count)
                return; //at the end
            
            ScrollIcon iconToSelect = icons[desiredIconIndex];
            OnClickIcon(iconToSelect);
        }

        public void SnapToPreviousItem()
        {
            int desiredIndex = lastSelectedIconIndex - 1;
            if (desiredIndex == -1)
            {
                desiredIndex = icons.Count - 1; //wrap
            }

            int desiredItemIndex = lastSelectedItemIndex - 1;
            if (!CanInfiniteScroll && desiredItemIndex < 0)
                return; //at the end
            
            ScrollIcon iconToSelect = icons[desiredIndex];
            OnClickIcon(iconToSelect);
        }

        public void SetItems(List<ScrollItem> newItems, int itemToSelectIndex = 0)
        {
            items = newItems;

            gameObject.SetActive(newItems.Count > 0);
            if (newItems.Count == 0)
            {
                return; //no items to show
            }

            if (!initialised)
            {
                Initialise();
            }
            else
            {
                //reset icons to initial positions and disable any additional in case where not infinite scrolling and is last item
                for (int count = 0; count < icons.Count; count++)
                {
                    ScrollIcon icon = icons[count];
                    icon.RectTransform.anchoredPosition = initialIconPositions[count];
                }
            }

            snapToItemOffscreenCoroutine = null;
            selectedItemWhenPointerDown = -1;
            clickedToSelect = false;
            
            //calculate the movement difference:
            Vector2 oldDistance = movementOffset; //previously tracked movement offset
            Vector2 newDistance = distanceBetweenIcons * itemToSelectIndex; //the new desired movement offset
            Vector2 offset = -(oldDistance + newDistance); //the amount being moved
            OnMovement(offset); //send event

            LoadItemsIntoIcons(itemToSelectIndex);
            SelectIconInstantly(newItems[itemToSelectIndex].CurrentIcon, false); //go to first item

            CheckForExtendedIcons();
        }

        /// <summary>
        /// Selects the specified icon instantly (without tweens).
        /// </summary>
        /// <param name="iconToSelect">The icon that's snapped to the magnet.</param>
        /// <param name="callMoveEvent">Whether the OnMoveAllItems event should be called.</param>
        public void SelectIconInstantly(ScrollIcon iconToSelect, bool callMoveEvent = true)
        {
            if (isLoggingEnabled) Debug.Log("Attempting to select icon " + icons.IndexOf(iconToSelect));

            Vector2 offset = magnetPosition - iconToSelect.RectTransform.anchoredPosition;

            foreach (ScrollIcon icon in icons)
            {
                icon.RectTransform.anchoredPosition += offset;
            }

            if (callMoveEvent)
                OnMovement(offset);

            PostMovement();
            ResetIndexPositions(items.IndexOf(iconToSelect.CurrentItem)); //need to recalc in case of wrapping
        }

        /// <summary>
        /// Tweens all the icons so that the provided icon is in the middle.
        /// <remarks>This will be overriden by user input.</remarks>
        /// </summary>
        public void SnapIconToMagnet(ScrollIcon iconToSnapToMiddle)
        {
            if (isLoggingEnabled) Debug.Log("Snapping " + iconToSnapToMiddle.gameObject.name + " to magnet");

            iconSnappingToMiddle = iconToSnapToMiddle;
            Vector2 offset = magnetPosition - iconToSnapToMiddle.RectTransform.anchoredPosition;

            foreach (ScrollIcon icon in icons)
            {
                icon.TweenToPos(icon.RectTransform.anchoredPosition + offset, snapTime);
            }
        }

        public void CompleteAllTweens()
        {
            foreach (ScrollIcon icon in icons)
            {
                icon.CurrentTween.Complete();
            }
            
            PostMovement();
        }

        /// <summary>
        /// Snaps the item to the magnet.
        /// If the item is currently shown in an icon, snap to the item. Else, keep scrolling to the end until it is shown in an icon, and select it.
        /// <remarks>This will be overriden by user input.</remarks>
        /// </summary>
        public void SnapItemToMagnet(int itemIndex)
        {
            if (lastSelectedItemIndex == itemIndex)
                return; //already selected

            clickedToSelect = true;
            
            //check if the item is currently in an icon
            ScrollIcon icon = GetItemAsIcon(itemIndex);
            if (icon != null)
            {
                SnapIconToMagnet(icon);
                return;
            }

            int distanceForwards = GetDistanceToItem(itemIndex, SnapToItemDirection.FORWARD);
            int distanceBackwards = GetDistanceToItem(itemIndex, SnapToItemDirection.BACKWARD);
            
            SnapToItemDirection directionToMove;
            if (distanceBackwards == distanceForwards)
            {
                //pick a random direction
                int random = Random.Range(0, 2);
                directionToMove = random == 0 ? SnapToItemDirection.FORWARD : SnapToItemDirection.BACKWARD;
            }
            else if (distanceForwards < distanceBackwards)
            {
                directionToMove = SnapToItemDirection.FORWARD;
            }
            else
            {
                directionToMove = SnapToItemDirection.BACKWARD;
            }

            snapToItemOffscreenCoroutine = StartCoroutine(SnapToItemOffscreen(itemIndex, directionToMove));
        }
        
        
        /// <summary>
        /// Snaps the item to the magnet.
        /// If the item is currently shown in an icon, snap to the item. Else, keep scrolling to the end until it is shown in an icon, and select it.
        /// <remarks>This will be overriden by user input.</remarks>
        /// </summary>
        public void SnapItemToMagnet(ScrollItem item)
        {
            int itemIndex = items.IndexOf(item);
            SnapItemToMagnet(itemIndex);
        }

        public void OnStartScrolling()
        {
            if (items.Count == 0) return; //can't scroll if no items
            if (isScrolling) return;

            lastKnownDelta = Vector2.zero;
            foreach (ScrollIcon icon in icons)
            {
                icon.CurrentTween?.Kill();
            }

            ScrollUsing = this;
            isScrolling = true;

            if (isLoggingEnabled) Debug.Log("Started scrolling " + gameObject.name);
        }

        public void OnClickIcon(ScrollIcon icon)
        {
            clickedToSelect = true;
            icon.CurrentItem.OnClick();

            SnapIconToMagnet(icon);
        }

        public void OnStopScrolling()
        {
            if (!isScrolling) return;

            if (ScrollUsing == this)
            {
                ScrollUsing = null;
            }

            isScrolling = false;
            isDecelerating = true;

            timeOnPointerUp = Time.realtimeSinceStartup;

            if (isLoggingEnabled) Debug.Log("Stopped scrolling " + gameObject.name);
        }

        public float GetItemSpacing()
        {
            RectTransform prefabRect = scrollIconPrefab.GetComponent<RectTransform>();
            float iconSize = IsHorizontal ? prefabRect.sizeDelta.x : prefabRect.sizeDelta.y;
            float scrollSize = Mathf.Abs(IsHorizontal ? scrollMinPos.x : scrollMinPos.y);
            return (scrollSize - iconSize * numberOfIcons) / numberOfIcons;
        }

        public void SetInteractable(bool interactable)
        {
            this.interactable = interactable;
        }

        public void ToggleUseInfiniteScroll(bool enable)
        {
            useInfiniteScroll = enable;
            CheckToDisableInfiniteScroll();
        }

        #endregion


        #region PRIVATE METHODS

        private void Initialise()
        {
            if (items.Count == 0 && preDefinedItems.Value.Length == 0)
            {
                //no items to show
                return;
            }

            initialised = true;

            rectTransform = GetComponent<RectTransform>();

            scrollMaxPos = Vector2.zero;
            scrollMinPos = IsVertical ? -rectTransform.rect.size : rectTransform.rect.size;

            if (populatesAtRuntime)
            {
                Populate();
            }
            else
            {
                SetupPreDefinedItems();
            }

            SetInitialIconPositions();

            ResetIndexPositions();
            if (items.Count > 0) //items might not be initialised yet
            {
                SelectIconInstantly(icons[0], false);
            }

            CacheDistanceBetweenIcons();
        }

        private void SetInitialIconPositions()
        {
            initialIconPositions.Clear();
            foreach (ScrollIcon icon in icons)
            {
                initialIconPositions.Add(icon.RectTransform.anchoredPosition);
            }
        }

        private void CacheDistanceBetweenIcons()
        {
            Vector2 difference = icons[0].RectTransform.anchoredPosition - icons[1].RectTransform.anchoredPosition;
            distanceBetweenIcons = new Vector2(Mathf.Abs(difference.x), Mathf.Abs(difference.y));
        }

        /// <summary>
        /// Check in case where not infinite scrolling and icon extends past the last item, and disables it.
        /// </summary>
        private void CheckForExtendedIcons()
        {
            extendedIcons.Clear(); //reset
            if (!CanInfiniteScroll)
            {
                int itemIndexCount = firstItemIndexShowing;
                foreach (ScrollIcon icon in icons)
                {
                    bool extendsPastLastItem = itemIndexCount >= items.Count;
                    if (extendsPastLastItem)
                    {
                        icon.gameObject.SetActive(false);
                        extendedIcons.Add(icon);
                    }

                    itemIndexCount++;
                }
            }
        }

        /// <summary>
        /// Checks to disable infinite scroll in case there's not enough items to fill all the icons.
        /// </summary>
        private void CheckToDisableInfiniteScroll()
        {
            if (!useInfiniteScroll || !disableInfiniteScrollIfNotEnoughItems)
            {
                return;
            }

            bool needToEnable = !isInfiniteScrollEnabled && items.Count >= icons.Count;
            bool needToDisable = isInfiniteScrollEnabled && items.Count < icons.Count;
            if (needToEnable)
            {
                EnableInfiniteScroll();
            }
            else if (needToDisable)
            {
                EnableInfiniteScroll(false);
            }
        }

        private void CheckToDecelerate()
        {
            if (isScrolling)
            {
                return;
            }

            float timeSincePointerUp = Time.realtimeSinceStartup - timeOnPointerUp;

            if (timeSincePointerUp <= decelerationTime)
            {
                Decelerate(timeSincePointerUp);
            }
            else if (isDecelerating)
            {
                OnStopDecelerating();
            }
        }

        private void EnableInfiniteScroll(bool enable = true)
        {
            if (isLoggingEnabled) Debug.Log((enable ? "Enabling" : "Disabling") + " infinite scroll on " + gameObject.name + ".");
            isInfiniteScrollEnabled = enable;
        }

        /// <summary>
        /// Make sure to include tweens in the 'OnMoveAllItems' event.
        /// Check if there was movement this frame and call the OnMoveAllItems event.
        /// </summary>
        private void CheckForTweenToCallMoveEvent()
        {
            if (icons.Count != 0 && ClosestIconToMagnet != null)
            {
                Vector2 pos = icons[0].RectTransform.anchoredPosition;
                if (isTweening)
                {
                    icons[0].CurrentTween.OnUpdate(() => OnMovement(icons[0].RectTransform.anchoredPosition - lastKnownPositionOfFirstIcon));
                }

                lastKnownPositionOfFirstIcon = pos;
            }
        }

        private void PostMovement()
        {
            foreach (ScrollIcon icon in icons)
            {
                CheckToWrap(icon);
                ApplyIndividualEffects(icon);
            }

            CheckIfSelectedIcon();
            ApplyGlobalEffects();
        }

        /// <summary>
        /// Loads the items from startIndex into the icons
        /// </summary>
        private void LoadItemsIntoIcons(int itemStartIndex)
        {
            ResetIndexPositions(itemStartIndex);

            //update icons in use
            iconsInUse = items.Count > icons.Count ? icons.Count : items.Count;

            if (items.Count < icons.Count && !CanInfiniteScroll)
            {
                if (isLoggingEnabled) Debug.Log(gameObject.name + " doesn't have enough items to fill all the icons!");
                itemStartIndex = 0; //always start at 0 for scrolls that don't have enough items to keep the order
            }
            else
            {
                itemStartIndex = firstItemIndexShowing;
            }

            for (int count = 0; count < icons.Count; count++)
            {
                ScrollIcon icon = icons[count];
                if (count >= iconsInUse && !CanInfiniteScroll)
                {
                    //disable the icon if no item to fit
                    icon.gameObject.SetActive(false);
                    continue;
                }

                icon.gameObject.SetActive(true); //make sure it's enabled in case it was disabled

                if (itemStartIndex >= items.Count)
                {
                    //wrap
                    itemStartIndex = 0;
                }

                items[itemStartIndex].OnLoad(icon);

                itemStartIndex++;
            }
        }


        /// <summary>
        /// Resets the positions to startIndex
        /// </summary>
        private void ResetIndexPositions(int itemStartIndex = 0)
        {
            //check to disable/enable infinite scroll before loading
            CheckToDisableInfiniteScroll();

            //reset closest
            closestIconToMagnetIndex = -1;

            //initialise icon indexes
            firstIconIndex = 0;
            lastIconIndex = icons.Count - 1;

            //update the last icon index in case where infinite scroll is disabled
            int maxItemIndex = items.Count - 1;
            if (maxItemIndex < lastIconIndex && !CanInfiniteScroll) lastIconIndex = maxItemIndex;

            //initialise item indexes:

            //take any icons before
            int iconsBefore = Mathf.FloorToInt(icons.Count / 2f); //magnet is always in the middle
            
            firstItemIndexShowing = CanInfiniteScroll ? itemStartIndex - iconsBefore : 0;
            if (firstItemIndexShowing < 0)
            {
                int wrappedFirstItemIndex = itemStartIndex;
                for (int count = 0; count < iconsBefore; count++)
                {
                    wrappedFirstItemIndex--;
                    if (wrappedFirstItemIndex < 0)
                        wrappedFirstItemIndex = items.Count - 1; //wrap
                }

                firstItemIndexShowing = wrappedFirstItemIndex;
            }

            lastItemIndexShowing = CanInfiniteScroll ? itemStartIndex + iconsBefore : firstItemIndexShowing + icons.Count - 1;
            if (lastItemIndexShowing > items.Count - 1)
            {
                if (CanInfiniteScroll)
                    lastItemIndexShowing = items.Count == 1 ? 0 : (lastItemIndexShowing % maxItemIndex) - 1; //wrap
                else lastItemIndexShowing = items.Count - 1;
            }

            if (isLoggingEnabled) Debug.Log("Reset index positions for " + gameObject.name + " to " + firstIconIndex + "-" + lastIconIndex + " / " + firstItemIndexShowing + "-" + lastItemIndexShowing);
        }

        private void Populate()
        {
            //get the spacing
            RectTransform prefabRect = scrollIconPrefab.GetComponent<RectTransform>();
            float iconSize = IsHorizontal ? prefabRect.sizeDelta.x : prefabRect.sizeDelta.y;
            float scrollSize = Mathf.Abs(IsHorizontal ? scrollMinPos.x : scrollMinPos.y);
            float iconSpacing = (scrollSize - iconSize * numberOfIcons) / numberOfIcons;

            //instantiate enough icons and initialise the positions
            float totalSpacing = 0;
            for (int count = 0; count < numberOfIcons; count++)
            {
                ScrollIcon icon = Instantiate(scrollIconPrefab, transform);
                icon.name = icon.name + " " + totalSpacing;
                icon.Initialise(this);

                icon.RectTransform.anchoredPosition =
                    new Vector2(IsHorizontal ? totalSpacing : 0, IsVertical ? totalSpacing : 0);

                //set spacing for next icon:
                totalSpacing += IsHorizontal
                    ? icon.RectTransform.sizeDelta.x + iconSpacing
                    : -icon.RectTransform.sizeDelta.y - iconSpacing;

                icons.Add(icon);
            }
        }

        private void SetupPreDefinedItems()
        {
            items.Clear();
            foreach (PreDefinedScrollItem item in preDefinedItems.Value)
            {
                items.Add(item.scrollItem);
                icons.Add(item.scrollIcon);
                item.scrollIcon.SetCurrentItem(item.scrollItem);
                item.scrollIcon.Initialise(this);
                item.scrollItem.OnLoad(item.scrollIcon);
            }
        }
        
        private int GetDistanceToItem(int itemIndex, SnapToItemDirection direction)
        {
            if (direction == SnapToItemDirection.BACKWARD)
            {
                int distanceForwards = 1;
                for (int count = lastSelectedItemIndex + 1; count < lastSelectedItemIndex + items.Count; count++)
                {
                    int wrapped = count;
                    if (wrapped >= items.Count)
                        wrapped -= items.Count;

                    if (wrapped == itemIndex)
                        return distanceForwards;

                    distanceForwards++;
                }
            }
            else
            {
                int distanceBackwards = 1;
                for (int count = lastSelectedItemIndex - 1; count > lastSelectedItemIndex - items.Count; count--)
                {
                    int wrapped = count;
                    if (wrapped <= -1)
                        wrapped += items.Count;
                
                    if (wrapped == itemIndex)
                        return distanceBackwards;

                    distanceBackwards++;
                }
            }

            throw new InvalidOperationException($"Could not find distance to item {itemIndex} in {gameObject.name}");
        }

        /// <summary>
        /// Send the movement changes to listeners.
        /// </summary>
        /// <param name="offset"></param>
        private void OnMovement(Vector2 offset)
        {
            movementOffset += offset;

            if (isLoggingEnabled) Debug.Log("(" + gameObject.name + ") Movement: " + offset + "(total = " + movementOffset + ")");

            OnMoveAllItems?.Invoke(offset);
        }

        /// <returns>The closest item and it's distance to the position</returns>
        private (ScrollIcon, float) GetClosestIconToPosition(Vector2 position)
        {
            float smallestDistance = float.MaxValue;
            ScrollIcon closestIcon = null;
            foreach (ScrollIcon icon in icons)
            {
                if (!icon.gameObject.activeSelf) continue;

                float distance = Vector2.Distance(icon.RectTransform.anchoredPosition, position);
                if (distance < smallestDistance)
                {
                    closestIcon = icon;
                    smallestDistance = distance;
                }
            }

            return (closestIcon, smallestDistance);
        }

        /// <summary>
        /// Gets the icon that the item is populated in.
        /// </summary>
        /// <returns>The icon that the item is populated in, or null if not shown.</returns>
        private ScrollIcon GetItemAsIcon(int itemIndex)
        {
            foreach (ScrollIcon icon in icons)
            {
                int index = items.IndexOf(icon.CurrentItem);
                if (index == itemIndex)
                    return icon;
            }

            return null;
        }

        /// <summary>
        /// Snaps to an item that isn't in an icon (offscreen) in the specified direction.
        /// </summary>
        private IEnumerator SnapToItemOffscreen(int itemIndex, SnapToItemDirection directionToMove)
        {
            int lastTrackedSelectedIndex = lastSelectedItemIndex;

            //start by moving to the furthest icon in the specified direction
            SnapIconToMagnet(directionToMove == SnapToItemDirection.FORWARD ? icons[firstIconIndex] : icons[lastIconIndex]);

            for (int count = 0; count <= items.Count - 1; count++) //we want to repeat until true, but to avoid infinite loop just check the max amount (item count)
            {
                //wait for the selected item to change
                int finalIndex = lastTrackedSelectedIndex;
                yield return new WaitUntil(() => finalIndex != lastSelectedItemIndex);
                lastTrackedSelectedIndex = lastSelectedItemIndex;

                //check if the item is now in an icon
                ScrollIcon icon = GetItemAsIcon(itemIndex);
                if (icon != null)
                {
                    //the icon has been populated, so snap to the icon
                    SnapIconToMagnet(icon);
                    yield break;
                }

                //continue towards the furthest icon
                SnapIconToMagnet(directionToMove == SnapToItemDirection.FORWARD ? icons[firstIconIndex] : icons[lastIconIndex]);
            }

            //shouldn't get here, but just in case log an error that something isn't right.
            Debug.LogError("Tried snapping to offscreen item but the item wasn't found.");
        }

        private void ApplyIndividualEffects(ScrollIcon icon)
        {
            if (effects.Length > 0)
            {
                Vector2 currentPos = icon.RectTransform.anchoredPosition;

                float percent = IsHorizontal ? currentPos.x / scrollMinPos.x : currentPos.y / scrollMinPos.y;

                //clamp the percent
                percent = Mathf.Clamp01(percent);

                foreach (ScrollEffect scrollEffect in effects)
                {
                    scrollEffect.ApplyEffectToIcon(direction, icon, percent);
                }
            }
        }

        private void ApplyGlobalEffects()
        {
            if (useOutlineEffect)
            {
                ApplyOutlineEffect();
            }
        }

        #region GLOBAL EFFECTS

        private void ApplyOutlineEffect()
        {
            //move the outline effect to the current selected icon
            ScrollIcon closestIconToMagnet = icons[ClosestIconToMagnetIndex];
            Vector2 newPos = closestIconToMagnet.RectTransform.anchoredPosition
                             + closestIconToMagnet.ImageComponent.rectTransform.anchoredPosition; //add the image rect in case there's an offset effect
            outlineEffectImage.rectTransform.anchoredPosition = newPos;

            float percent = GetOutlineEffectPercent();

            float finalScale = outlineEffectMinScale + (percent * (outlineEffectMaxScale - outlineEffectMinScale));
            outlineEffectImage.rectTransform.localScale = new Vector2(finalScale, finalScale);
        }

        private float GetOutlineEffectPercent()
        {
            if (icons.Count <= 1)
            {
                return 0; //not enough icons to use effect
            }

            //set the scale based on the distance to the centre
            int iconIndexMovingTo = GetIconIndexMovingTo();
            float maxDistance;
            int maxIconIndex = items.Count - 1 < icons.Count - 1 ? items.Count - 1 : icons.Count - 1;

            //handle case where not infinite scrolling, and going below min/max icons:
            if (iconIndexMovingTo < 0 || iconIndexMovingTo > maxIconIndex)
            {
                //just use same spacing from 0 to 1
                maxDistance = IsHorizontal ? distanceBetweenIcons.x / 2 : distanceBetweenIcons.y / 2;
            }
            else
            {
                maxDistance = GetSpacingBetweenIcons(ClosestIconToMagnetIndex, iconIndexMovingTo) / 2;
            }

            float existingPos = IsHorizontal ? outlineEffectImage.rectTransform.anchoredPosition.x : outlineEffectImage.rectTransform.anchoredPosition.y;
            float centre = IsHorizontal ? (scrollMinPos / 2).x : (scrollMinPos / 2).y;
            float percent = 1 - (Mathf.Abs(existingPos - centre) / maxDistance);
            return Mathf.Clamp01(percent);
        }

        #endregion

        private int GetIconIndexMovingTo()
        {
            float movementInDirection = IsHorizontal ? movement.x : movement.y;
            int nextIconIndex = movementInDirection > 0 ? ClosestIconToMagnetIndex + 1 : ClosestIconToMagnetIndex - 1;
            if (nextIconIndex > icons.Count - 1 && CanInfiniteScroll) nextIconIndex = 0; //wrapped - get the first icon
            if (nextIconIndex < 0 && CanInfiniteScroll)
            {
                nextIconIndex = icons.Count - 1; //wrapped - get the last icon
            }

            return nextIconIndex;
        }

        private float GetSpacingBetweenIcons(int iconIndex1, int iconIndex2)
        {
            Vector2 spacingVec = icons[iconIndex1].RectTransform.anchoredPosition - icons[iconIndex2].RectTransform.anchoredPosition;
            float spacing = IsHorizontal ? Mathf.Abs(spacingVec.x) : Mathf.Abs(spacingVec.y);
            return spacing;
        }

        private void OnStopDecelerating()
        {
            isDecelerating = false;

            //if snapping is enabled OR has passed first or final item (elasticity is enabled)
            if (!clickedToSelect && (useMagnetSnapping || currentElasticityModifier > 0))
            {
                SnapIconToMagnet(GetClosestIconToPosition(magnetPosition).Item1); //start snapping
            }
        }

        private void Decelerate(float timeSincePointerUp)
        {
            float percent = 1 - Mathf.Clamp01(timeSincePointerUp / decelerationTime);

            float xDeceleration = direction == Direction.HORIZONTAL ? lastKnownDelta.x * percent : 0;
            float yDeceleration = direction == Direction.VERTICAL ? lastKnownDelta.y * percent : 0;

            Vector2 deceleration = new Vector2(xDeceleration, yDeceleration);

            MoveAllItems(deceleration);
        }

        private void OnSelectIcon(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;

            if (oldIndex >= 0)
            {
                ScrollIcon unselectedIcon = icons[oldIndex];
                ScrollItem unselectedItem = unselectedIcon.CurrentItem;
                unselectedItem.OnUnselect();
                onSelectIcon?.Invoke();
            }

            //get the item that the icon has
            ScrollIcon selectedIcon = icons[newIndex];
            ScrollItem selectedItem = selectedIcon.CurrentItem;
            selectedItem.OnSelect();
            
            if (!isScrolling && !clickedToSelect)
                selectedItem.OnSelectComplete();
            
            lastSelectedItemIndex = items.IndexOf(selectedItem);
            lastSelectedIconIndex = newIndex;
        }

        private void MoveAllItems(Vector2 amount)
        {
            movement = GetModifiedMovement(amount * scrollSpeed);

            foreach (ScrollIcon icon in icons)
            {
                icon.RectTransform.anchoredPosition += movement;
            }

            OnMovement(movement);
        }

        private void CheckIfSelectedIcon()
        {
            //check if changed icon
            int closestIconIndex = icons.IndexOf(GetClosestIconToPosition(magnetPosition).Item1);
            if (closestIconIndex != ClosestIconToMagnetIndex)
            {
                OnSelectIcon(ClosestIconToMagnetIndex, closestIconIndex);
                closestIconToMagnetIndex = closestIconIndex;
            }
        }

        private void CheckToWrap(ScrollIcon icon)
        {
            if (!icon.CurrentItem.HasLoaded)
            {
                return;
            }

            if (items.Count < icons.Count && !CanInfiniteScroll)
            {
                //no wrapping if not enough items to fill icons
                return;
            }

            Vector2 existingPos = icon.RectTransform.anchoredPosition;

            if (IsPastMinPos(existingPos))
            {
                if (!CanInfiniteScroll && firstItemIndexShowing <= 0)
                {
                    //don't wrap if at first item and not infinite scrolling
                    return;
                }

                //wrap to max pos
                Vector2 difference = existingPos - scrollMinPos;
                icon.RectTransform.anchoredPosition = new Vector2(direction == Direction.HORIZONTAL ? scrollMaxPos.x + difference.x : existingPos.x,
                    direction == Direction.VERTICAL ? scrollMaxPos.y + difference.y : existingPos.y);

                OnWrap(icon);

                //set new first icon (the wrapped icon)
                firstIconIndex = icons.IndexOf(icon);
                //also set last icon
                lastIconIndex = firstIconIndex - 1;
                if (lastIconIndex == -1) lastIconIndex = icons.Count - 1;

                //update the item for the wrapped icon
                firstItemIndexShowing--;
                if (firstItemIndexShowing == -1) firstItemIndexShowing = items.Count - 1; //infinite scrolling, go back to last item
                //also set the last item index
                lastItemIndexShowing--;
                if (lastItemIndexShowing == -1) lastItemIndexShowing = items.Count - 1; //infinite scrolling, go back to last item

                if (isLoggingEnabled) Debug.Log(icon.gameObject.name + " is wrapping from bottom to top in " + gameObject.name + " (item " + firstItemIndexShowing + " in " + firstIconIndex + ")");

                //update the icon
                items[firstItemIndexShowing].OnLoad(icons[firstIconIndex]);
            }
            else if (IsPastMaxPos(existingPos))
            {
                if (!CanInfiniteScroll && lastItemIndexShowing >= items.Count - 1)
                {
                    //don't wrap if at last item and not infinite scrolling
                    return;
                }

                //wrap to min pos
                Vector2 difference = existingPos - scrollMaxPos;
                icon.RectTransform.anchoredPosition = new Vector2(direction == Direction.HORIZONTAL ? scrollMinPos.x + difference.x : existingPos.x,
                    direction == Direction.VERTICAL ? scrollMinPos.y + difference.y : existingPos.y);

                OnWrap(icon);

                //set new last icon (the wrapped icon)
                lastIconIndex = icons.IndexOf(icon);
                //also set first icon
                firstIconIndex = lastIconIndex + 1;
                if (firstIconIndex == icons.Count) firstIconIndex = 0;

                //update the item for the wrapped icon
                lastItemIndexShowing++;
                if (lastItemIndexShowing == items.Count) lastItemIndexShowing = 0; //infinite scrolling, go back to item 0
                //also set the first item index
                firstItemIndexShowing++;
                if (firstItemIndexShowing == items.Count) firstItemIndexShowing = 0; //infinite scrolling, go back to item 0

                if (isLoggingEnabled) Debug.Log(icon.gameObject.name + " is wrapping from top to bottom in " + gameObject.name + " (item " + lastItemIndexShowing + " in " + lastIconIndex + ")");

                //update the icon
                items[lastItemIndexShowing].OnLoad(icons[lastIconIndex]);
            }
        }

        /// <summary>
        /// Called when the supplied icon is wrapped.
        /// </summary>
        private void OnWrap(ScrollIcon icon)
        {
            //check to reset positioning
            if (isTweening && icons[0].CurrentTween.IsPlaying())
            {
                SnapIconToMagnet(iconSnappingToMiddle); //kill the tweens and restart with the new position
            }

            //check to reenable extended icons that were disabled
            if (extendedIcons.Contains(icon))
            {
                icon.gameObject.SetActive(true);
                extendedIcons.Remove(icon);
            }
        }

        private bool IsPastMaxPos(Vector2 position)
        {
            return (IsHorizontal && position.x < scrollMaxPos.x)
                   || (IsVertical && position.y > scrollMaxPos.y);
        }

        private bool IsPastMinPos(Vector2 position)
        {
            return (IsHorizontal && position.x > scrollMinPos.x)
                   || (IsVertical && position.y < scrollMinPos.y);
        }

        private Vector2 GetModifiedMovement(Vector2 original)
        {
            //add elasticity and clamp to max speed
            return new Vector2(
                IsHorizontal ? GetElasticity(original.x) : 0,
                IsVertical ? GetElasticity(original.y) : 0);
        }

        private float GetElasticity(float originalAmount)
        {
            if (CanInfiniteScroll)
            {
                //no elasticity when infinite scroll
                currentElasticityModifier = 0;
                return originalAmount;
            }

            ScrollIcon firstIcon = icons[firstIconIndex];
            ScrollIcon lastActiveIcon = GetLastActiveIcon(); //ignore any icons that are inactive at the end

            float firstItemPosValue = IsHorizontal
                ? lastActiveIcon.RectTransform.anchoredPosition.x
                : firstIcon.RectTransform.anchoredPosition.y;
            float lastItemPosValue = IsHorizontal
                ? firstIcon.RectTransform.anchoredPosition.x
                : lastActiveIcon.RectTransform.anchoredPosition.y;

            float magnetPos = IsHorizontal ? magnetPosition.x : magnetPosition.y;
            currentElasticityModifier = originalAmount switch
            {
                < 0 => firstItemPosValue < magnetPos ? magnetPos - firstItemPosValue : 0,
                > 0 => lastItemPosValue > magnetPos ? lastItemPosValue - magnetPos : 0,
                _ => 0
            };

            float finalMovement = currentElasticityModifier > 0 ? originalAmount / (currentElasticityModifier / elasticity) : originalAmount;

            if (isLoggingEnabled) Debug.Log("Elasticity modifier is " + finalMovement + " (original = " + originalAmount + ")");
            return finalMovement;
        }

        private ScrollIcon GetLastActiveIcon()
        {
            int count = lastIconIndex;
            while (true)
            {
                ScrollIcon icon = icons[count];
                if (icon.gameObject.activeSelf)
                {
                    return icon;
                }

                count--;
                if (count < 0) count = icons.Count - 1; //wrap
            }
        }

        #endregion

    }
}