using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    /// <summary>
    /// Scales the specified RectTransforms to fit inside of this RectTransform.
    /// </summary>
    [ExecuteAlways]
    public class ScaleRectsToFit : MonoBehaviour
    {

        [SerializeField] private Vector2 defaultScale = new(1,1);

        [SerializeField] private bool preserveAspectRatio = true;
        [SerializeField] private bool isHorizontal;
        [SerializeField] private bool isVertical;

        [SerializeField, ReadOnly, ConditionalField(nameof(isHorizontal))] private float currentHorizontalPercent;
        [SerializeField, ReadOnly, ConditionalField(nameof(isVertical))] private float currentVerticalPercent;

        private RectTransform rectTransform => transform as RectTransform;
        
        private void Update()
        {
            CheckToScale();
        }

        private void CheckToScale()
        {
            if (transform.childCount == 0)
                return;
            
            Vector2 totalSize = Vector2.zero;
            foreach (RectTransform rectToScale in transform)
            {
                totalSize += rectToScale.rect.size;
            }

            //include spacing
            HorizontalLayoutGroup horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayoutGroup != null)
                totalSize = totalSize.OffsetX(horizontalLayoutGroup.spacing * (transform.childCount - 1));
            VerticalLayoutGroup verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
                totalSize = totalSize.OffsetY(verticalLayoutGroup.spacing * (transform.childCount - 1));
            
            Vector2 currentSize = rectTransform.rect.size;

            currentHorizontalPercent = currentSize.x / totalSize.x;
            currentVerticalPercent = currentSize.y / totalSize.y;
            
            float desiredScaleX = totalSize.x > currentSize.x ? currentHorizontalPercent * defaultScale.x : defaultScale.x;
            float desiredScaleY = totalSize.y > currentSize.y ? currentVerticalPercent * defaultScale.y : defaultScale.y;
            
            Vector3 desiredScale = new Vector3(isHorizontal ? desiredScaleX : 
                    (preserveAspectRatio && isVertical ? desiredScaleY : rectTransform.localScale.x), 
                isVertical ? desiredScaleY :
                    (preserveAspectRatio && isHorizontal ? desiredScaleX : rectTransform.localScale.y),
                rectTransform.localScale.z);
                
            rectTransform.localScale = desiredScale;
            
            //if scaling down, move to the left to account for scaling
            float excessSize = totalSize.x - currentSize.x;
            if (horizontalLayoutGroup != null)
            {
                int desiredPadding = currentHorizontalPercent < 1 ? Mathf.RoundToInt(-excessSize) : 0;
                if (horizontalLayoutGroup.padding.left != desiredPadding)
                {
                    horizontalLayoutGroup.padding.left = desiredPadding; //TODO: may want to handle if padding is not 0
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
            if (verticalLayoutGroup != null)
            {
                int desiredPadding = currentVerticalPercent < 1 ? Mathf.RoundToInt(-excessSize) : 0;
                if (verticalLayoutGroup.padding.top != desiredPadding)
                {
                    verticalLayoutGroup.padding.top = desiredPadding; //TODO: may want to handle if padding is not 0
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
        }

    }
}
