using System;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(RectTransform))]
    public class MoveRectTransformWithMagneticScroll : MonoBehaviour
    {
        [SerializeField] private float speedModifier = 1;
        [SerializeField] private MagneticScroll magneticScroll;

        private RectTransform rectTransform => (RectTransform)transform;

        private bool isInitialised;

        private void Initialise()
        {
            isInitialised = true;

            magneticScroll.OnMoveAllItems += OnMoveAllItems;
        }

        private void Awake()
        {
            if (!isInitialised)
                Initialise();
        }

        private void OnDestroy()
        {
            magneticScroll.OnMoveAllItems -= OnMoveAllItems;
        }

        /// <summary>
        /// Sets the anchored position of the rect transform to be like an additional item in the magnetic scroll, as the last item.
        /// Useful for adding additional components like a 'done' button.
        /// </summary>
        public void SetPositionAsLastItem()
        {
            CoroutineHelper.Instance.PerformAfterTrue(ScrollIconsReady, () =>
            {
                var thisItem = rectTransform;
                var firstItem = (RectTransform)magneticScroll.Items[0].CurrentIcon.transform;
                // Since magnetic scrolls often use wrapping, base this value on the total items the mag holds, not the instantiated gameobjects
                var spacing = magneticScroll.GetItemSpacing();
                var offset = Vector2.down * ((firstItem.sizeDelta.y + spacing) * magneticScroll.Items.Count);
                thisItem.anchoredPosition = firstItem.anchoredPosition + offset;
            });
        }

        private bool ScrollIconsReady()
        {
            return magneticScroll.Items.Count > 0 && magneticScroll.Items[0].CurrentIcon != null;
        }

        private void OnMoveAllItems(Vector2 amount)
        {
            rectTransform.anchoredPosition += amount * speedModifier;
        }
    }
}