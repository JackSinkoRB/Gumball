using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
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

        [Header("Properties tab")]
        [SerializeField] private GameObject properties;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private PerformanceRatingSlider maxSpeedSlider;
        [SerializeField] private PerformanceRatingSlider accelerationSlider;
        [SerializeField] private PerformanceRatingSlider handlingSlider;
        [SerializeField] private PerformanceRatingSlider nosSlider;
        
        [Header("Events tab")]
        [SerializeField] private GameObject events;
        [SerializeField] private Transform eventHolder;
        [SerializeField] private EventScrollItem eventPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private int currentSelected;

        private SwapCorePartOptionButton currentPartOption => PanelManager.GetPanel<SwapCorePartPanel>().SelectedOption;
        private AICar currentCar => WarehouseManager.Instance.CurrentCar;
        
        public int CurrentSelected => currentSelected;

        public void Select(int option)
        {
            foreach (CorePart corePaart in CorePartManager.AllParts)
                corePaart.SetUnlocked(true);
            
            properties.SetActive(option == 0 && currentPartOption != null);
            events.SetActive(option == 1 && currentPartOption != null);

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

            if (currentPartOption != null)
            {
                UpdateDescription();
                UpdateEvents();
                UpdatePerformanceRatingSliders();
            }
        }
        
        private void UpdateDescription()
        {
            descriptionLabel.text = currentPartOption.CorePart.Description;
        }

        private void UpdateEvents()
        {
            foreach (Transform child in eventHolder)
                child.gameObject.Pool();

            foreach (GameSession session in currentPartOption.CorePart.SessionsThatGiveReward)
            {
                EventScrollItem eventScrollItem = eventPrefab.gameObject.GetSpareOrCreate<EventScrollItem>(eventHolder);
                eventScrollItem.transform.SetAsLastSibling();
                eventScrollItem.Initialise(session, currentPartOption.CorePart);
            }
        }

        private void UpdatePerformanceRatingSliders()
        {
            //create a profile with the specific core part
            Dictionary<CorePart.PartType, CorePart> allParts = CorePartManager.GetCoreParts(currentCar.CarIndex);
            allParts[currentPartOption.CorePart.Type] = currentPartOption.CorePart;

            CarPerformanceProfile profileWithPart = new CarPerformanceProfile(allParts.Values);
            
            maxSpeedSlider.UpdateProfile(currentCar.PerformanceSettings, profileWithPart);
            accelerationSlider.UpdateProfile(currentCar.PerformanceSettings, profileWithPart);
            handlingSlider.UpdateProfile(currentCar.PerformanceSettings, profileWithPart);
            nosSlider.UpdateProfile(currentCar.PerformanceSettings, profileWithPart);
        }

    }
}
