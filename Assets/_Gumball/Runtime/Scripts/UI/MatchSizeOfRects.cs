using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [ExecuteAlways]
    public class MatchSizeOfRects : MonoBehaviour
    {

        [HelpBox("It will match the size of all the children, but you can assign additional objects here.", position: HelpBoxAttribute.Position.ABOVE)]
        [SerializeField] private RectTransform[] additionalRects = Array.Empty<RectTransform>();
        [Space(5)]
        [SerializeField] private bool isHorizontal;
        [SerializeField] private bool isVertical;
        [Space(5)]
        [SerializeField, ConditionalField(nameof(isHorizontal))] private float additionalWidth;
        [SerializeField, ConditionalField(nameof(isVertical))] private float additionalHeight;

        private RectTransform rectTransform => transform as RectTransform;

        private void LateUpdate()
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (!isHorizontal && !isVertical)
                return;
            
            float totalWidth = 0;
            float totalHeight = 0;
            foreach (RectTransform additionalObject in additionalRects)
            {
                if (!additionalObject.gameObject.activeSelf)
                    continue;
                
                totalWidth += additionalObject.rect.width;
                totalHeight += additionalObject.rect.height;
            }

            totalWidth += additionalWidth;
            totalHeight += additionalHeight;
            
            //get spacing from layout group
            HorizontalLayoutGroup horizontalLayoutGroup = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayoutGroup != null && isHorizontal && horizontalLayoutGroup.isActiveAndEnabled)
            {
                totalWidth += horizontalLayoutGroup.spacing * ((transform.childCount - 1) + additionalRects.Length);
                totalWidth += horizontalLayoutGroup.padding.left + horizontalLayoutGroup.padding.right;
            }
            VerticalLayoutGroup verticalLayoutGroup = gameObject.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null && isVertical && verticalLayoutGroup.isActiveAndEnabled)
            {
                totalHeight += verticalLayoutGroup.spacing * ((transform.childCount - 1) + additionalRects.Length);
                totalHeight += verticalLayoutGroup.padding.bottom + verticalLayoutGroup.padding.top;
            }

            rectTransform.sizeDelta = new Vector2(isHorizontal ? totalWidth : rectTransform.rect.width, 
                isVertical ? totalHeight : rectTransform.rect.height);
        }

    }
}
