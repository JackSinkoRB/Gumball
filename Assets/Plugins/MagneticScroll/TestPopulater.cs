using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagneticScrollUtils
{
    public class TestPopulater : MonoBehaviour
    {

        [SerializeField] private MagneticScroll magneticScroll;
        [SerializeField] private Color[] exampleColors;

        [Tooltip("Should the item index order be saved if the scroll is repopulated?")]
        [SerializeField] private bool keepItemOrderOnRepopulate;

        private void Start()
        {
            PopulateIcons();
        }

        private void PopulateIcons()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            foreach (Color color in exampleColors)
            {
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.color = color;

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems, keepItemOrderOnRepopulate ? magneticScroll.LastSelectedItemIndex : 0);
        }

    }
}