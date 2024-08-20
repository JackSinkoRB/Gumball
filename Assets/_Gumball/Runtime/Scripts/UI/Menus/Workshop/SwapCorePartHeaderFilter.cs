using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartHeaderFilter : MonoBehaviour
    {
        
        [SerializeField] private Transform filterButtonHolder;
        [SerializeField] private SwapCorePartFilterButton filterButtonPrefab;
        [SerializeField] private Color selectedFilterButtonColor = Color.white;
        [SerializeField] private Color deselectedFilterButtonColor = Color.grey;
        [SerializeField] private Image selector;
        [SerializeField] private Vector2 selectorPadding;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<SwapCorePartFilterButton> options = new();
        [SerializeField, ReadOnly] private CarType currentSelected;

        public CarType CurrentSelected => currentSelected;

        private void OnEnable()
        {
            InitialiseCategories();
        }

        public void Select(CarType carType)
        {
            SwapCorePartFilterButton selectedOption = options[(int)carType];
            RectTransform rectTransform = selectedOption.GetComponent<RectTransform>();
            selector.rectTransform.anchoredPosition = rectTransform.anchoredPosition;
            selector.rectTransform.sizeDelta = rectTransform.sizeDelta + selectorPadding;
            
            currentSelected = carType;

            foreach (SwapCorePartFilterButton filterButton in options)
            {
                AutosizeTextMeshPro label = filterButton.GetComponent<AutosizeTextMeshPro>();
                label.color = selectedOption == filterButton ? selectedFilterButtonColor : deselectedFilterButtonColor;
            }

            PanelManager.GetPanel<SwapCorePartPanel>().PopulateParts();
            PanelManager.GetPanel<SwapCorePartPanel>().SelectPartOption(null);
        }
        
        private void InitialiseCategories()
        {
            options.Clear();
            
            foreach (Transform child in filterButtonHolder)
                child.gameObject.Pool();
            
            foreach (CarType carType in Enum.GetValues(typeof(CarType)))
            {
                SwapCorePartFilterButton instance = filterButtonPrefab.gameObject.GetSpareOrCreate<SwapCorePartFilterButton>(filterButtonHolder);
                instance.Initialise(carType);
                instance.transform.SetAsLastSibling();

                Button button = instance.GetComponent<Button>(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Select(carType));
                
                options.Add(instance);
            }
            
        }
        
    }
}
