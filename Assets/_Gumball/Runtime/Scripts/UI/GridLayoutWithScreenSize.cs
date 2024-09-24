using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    public class GridLayoutWithScreenSize : MonoBehaviour
    {

        private enum LayoutDirection
        {
            HORIZONTAL,
            VERTICAL
        }

        [SerializeField] private LayoutDirection layoutDirection;
        [SerializeField, ConditionalField(nameof(layoutDirection), true)] private bool fitVertical;
        [SerializeField, ConditionalField(new[] {nameof(layoutDirection), nameof(fitVertical)}, new[] {true, false})] private float fitVerticalSpacing;
        [SerializeField, ConditionalField(nameof(layoutDirection))] private bool fitHorizontal;
        [SerializeField, ConditionalField(new[] {nameof(layoutDirection), nameof(fitHorizontal)}, new[] {false, false})] private float fitHorizontalSpacing;
        [SerializeField, ReadOnly(nameof(layoutDirection))] private int numberOfColumns = 1;
        [SerializeField, ReadOnly(nameof(layoutDirection), true)] private int numberOfRows = 1;

        [Space(5)]
        [Tooltip("The size of the elements as a percentage of the rect size.")]
        [SerializeField, Range(0.01f, 1)] private float elementSizeAsPercent;

        private RectTransform rectTransform => transform as RectTransform;
        
        private void OnValidate()
        {
            Resize();
        }

        private void Update()
        {
            if (!Application.isPlaying)
                Resize();
        }
        
        [ButtonMethod]
        public void Resize()
        {
            //changing the size of the elements

            float rectWidth = rectTransform.rect.width;
            float rectHeight = rectTransform.rect.height;

            if (layoutDirection == LayoutDirection.HORIZONTAL)
            {
                if (numberOfColumns == 0)
                    return;
                numberOfRows = Mathf.CeilToInt((float)transform.childCount / numberOfColumns);
            }
            else
            {
                if (numberOfRows == 0)
                    return;
                numberOfColumns = Mathf.CeilToInt((float)transform.childCount / numberOfRows);
            }

            float elementSize = layoutDirection == LayoutDirection.HORIZONTAL ? rectWidth * elementSizeAsPercent : rectHeight * elementSizeAsPercent;

            //fit vertical/horizontal:
            if (layoutDirection == LayoutDirection.HORIZONTAL && fitVertical)
            {
                rectHeight = elementSize * numberOfRows;
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectHeight);
            }
            
            if (layoutDirection == LayoutDirection.VERTICAL && fitHorizontal)
            {
                rectWidth = elementSize * numberOfRows;
                rectTransform.sizeDelta = new Vector2(rectWidth, rectTransform.sizeDelta.y);
            }
            
            //get spacing:
            Vector2 spacing = new Vector2(
                (rectWidth - (elementSize * numberOfColumns)) / (numberOfColumns - 1),
                (rectHeight - (elementSize * numberOfRows)) / (numberOfRows - 1));
            if (layoutDirection == LayoutDirection.HORIZONTAL && fitVertical)
                spacing = spacing.SetY(fitVerticalSpacing);
            if (layoutDirection == LayoutDirection.VERTICAL && fitHorizontal)
                spacing = spacing.SetX(fitHorizontalSpacing);
            
            int count = 0;
            foreach (RectTransform child in transform)
            {
                if (!child.gameObject.activeSelf)
                    continue;
                
                child.anchorMin = new Vector2(0, 1);
                child.anchorMax = new Vector2(0, 1);
                child.pivot = new Vector2(0, 1);
                
                child.sizeDelta = Vector2.one * elementSize;

                int column = count % numberOfColumns;
                int row = Mathf.FloorToInt((float)count / numberOfColumns);

                float xPos = (spacing.x * column) + (column * elementSize);
                float yPos = -(spacing.y * row) - (row * elementSize);
                
                child.anchoredPosition = new Vector2(xPos, yPos);

                count++;
            }
        }
        
    }
}
