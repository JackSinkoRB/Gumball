using System;
using UnityEngine;

namespace MagneticScrollUtils
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

        private void OnMoveAllItems(Vector2 amount)
        {
            rectTransform.anchoredPosition += amount * speedModifier;
        }
    }
}