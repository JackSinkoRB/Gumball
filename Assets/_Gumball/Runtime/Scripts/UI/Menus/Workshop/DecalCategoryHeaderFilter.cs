using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalCategoryHeaderFilter : MonoBehaviour
    {
        
        [SerializeField] private Transform filterButtonHolder;
        [SerializeField] private DecalCategoryFilterButton filterButtonPrefab;
        [SerializeField] private Color selectedFilterButtonColor = Color.white;
        [SerializeField] private Color deselectedFilterButtonColor = Color.grey;
        [SerializeField] private Image selector;
        [SerializeField] private Vector2 selectorPadding;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<DecalCategoryFilterButton> options = new();
        [SerializeField, ReadOnly] private DecalUICategory currentSelected;

        public DecalUICategory CurrentSelected => currentSelected;

        private void OnEnable()
        {
            InitialiseCategories();
        }

        public void Select(DecalUICategory category)
        {
            int categoryIndex = DecalManager.Instance.DecalUICategories.IndexOfItem(category);
            Select(categoryIndex);
        }

        public void Select(int categoryIndex)
        {
            DecalUICategory category = DecalManager.Instance.DecalUICategories[categoryIndex];
            DecalCategoryFilterButton selectedOption = options[categoryIndex];
            RectTransform rectTransform = selectedOption.GetComponent<RectTransform>();
            selector.rectTransform.anchoredPosition = rectTransform.anchoredPosition;
            selector.rectTransform.sizeDelta = rectTransform.sizeDelta + selectorPadding;
            
            currentSelected = category;

            foreach (DecalCategoryFilterButton filterButton in options)
            {
                AutosizeTextMeshPro label = filterButton.GetComponent<AutosizeTextMeshPro>();
                label.color = selectedOption == filterButton ? selectedFilterButtonColor : deselectedFilterButtonColor;
            }

            PanelManager.GetPanel<CreateLiveryTexturePanel>().PopulateDecals();
            PanelManager.GetPanel<CreateLiveryTexturePanel>().SelectDecalButton(null);
        }
        
        private void InitialiseCategories()
        {
            options.Clear();
            
            foreach (Transform child in filterButtonHolder)
                child.gameObject.Pool();
            
            foreach (DecalUICategory category in DecalManager.Instance.DecalUICategories)
            {
                DecalCategoryFilterButton instance = filterButtonPrefab.gameObject.GetSpareOrCreate<DecalCategoryFilterButton>(filterButtonHolder);
                instance.Initialise(category);
                instance.transform.SetAsLastSibling();

                Button button = instance.GetComponent<Button>(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Select(category));
                
                options.Add(instance);
            }
        }
        
    }
}
