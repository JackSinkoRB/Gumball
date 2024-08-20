using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartHeaderFilterInfo : MonoBehaviour
    {
        
        [SerializeField] private List<SwapCorePartFilterButton> options = new();
        [SerializeField] private Color selectedFilterButtonColor = Color.white;
        [SerializeField] private Color deselectedFilterButtonColor = Color.grey;
        [SerializeField] private Image selector;
        [SerializeField] private Vector2 selectorPadding;

        [SerializeField] private GameObject properties;
        [SerializeField] private GameObject events;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private int currentSelected;

        public void Select(int option)
        {
            SwapCorePartFilterButton selectedOption = options[option];
            RectTransform rectTransform = selectedOption.GetComponent<RectTransform>();
            selector.rectTransform.anchoredPosition = rectTransform.anchoredPosition;
            selector.rectTransform.sizeDelta = rectTransform.sizeDelta + selectorPadding;
            
            currentSelected = option;

            foreach (SwapCorePartFilterButton filterButton in options)
            {
                AutosizeTextMeshPro label = filterButton.GetComponent<AutosizeTextMeshPro>();
                label.color = selectedOption == filterButton ? selectedFilterButtonColor : deselectedFilterButtonColor;
            }

            ShowInfoProperties(option == 0);
            ShowInfoEvents(option == 1);
        }
        
        private void ShowInfoProperties(bool show)
        {
            properties.SetActive(show);

            if (show)
            {
                //update
            }
        }

        private void ShowInfoEvents(bool show)
        {
            events.SetActive(show);

            if (show)
            {
                //update
                
            }
        }

    }
}
