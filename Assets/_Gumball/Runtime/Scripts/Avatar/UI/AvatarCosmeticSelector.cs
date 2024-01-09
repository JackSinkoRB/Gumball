using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarCosmeticSelector : MonoBehaviour
    {

        [SerializeField] private AvatarEditorPanel editorPanel;
        [SerializeField] private MagneticScroll magneticScroll;
        
        private readonly Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> cosmeticsGrouped = new();
        
        private void OnEnable()
        {
            AvatarEditor.onSelectedAvatarChanged += OnSelectedAvatarChanged;
        }

        private void OnDisable()
        {
            AvatarEditor.onSelectedAvatarChanged -= OnSelectedAvatarChanged;
        }

        private void OnSelectedAvatarChanged(Avatar newAvatar)
        {
            GroupCosmeticsByCategory(newAvatar);
            SelectCategory(AvatarCosmeticCategory.Character);
        }
        
        public void SelectCategory(AvatarCosmeticCategory category)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            if (cosmeticsGrouped.ContainsKey(category))
            {
                foreach (AvatarCosmetic cosmetic in cosmeticsGrouped[category])
                {
                    ScrollItem scrollItem = new ScrollItem();
                    scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.sprite = cosmetic.Icon;
                    scrollItem.onSelectComplete += () => editorPanel.CosmeticDisplay.PopulateCosmeticOptions(cosmetic);

                    scrollItems.Add(scrollItem);
                }
            }

            magneticScroll.SetItems(scrollItems);

            //also populate the first cosmetic for the category
            AvatarCosmetic firstCosmeticInCategory = cosmeticsGrouped.ContainsKey(category) && cosmeticsGrouped[category].Count > 0 ? cosmeticsGrouped[category][0] : null;
            editorPanel.CosmeticDisplay.PopulateCosmeticOptions(firstCosmeticInCategory);
        }

        private void GroupCosmeticsByCategory(Avatar avatar)
        {
            cosmeticsGrouped.Clear();
            
            foreach (AvatarCosmetic cosmetic in avatar.CurrentBody.Cosmetics)
            {
                AvatarCosmeticCategory category = cosmetic.Category;
                List<AvatarCosmetic> avatarCosmeticsForCategory = new List<AvatarCosmetic>();
                if (cosmeticsGrouped.ContainsKey(category))
                    avatarCosmeticsForCategory.AddRange(cosmeticsGrouped[category]);
                
                avatarCosmeticsForCategory.Add(cosmetic);
                cosmeticsGrouped[category] = avatarCosmeticsForCategory;
            }
        }
        
        //
        // [SerializeField] private ScrollRect scrollRect;
        // [SerializeField] private GameObject categoryOptionPrefab;
        // [SerializeField] private Transform contentHolder;
        // [SerializeField] private Button backButton;
        //
        // [Header("Debugging")]
        // [SerializeField, ReadOnly] private AvatarUICategory selectedCategory;
        // [SerializeField, ReadOnly] private AvatarCosmetic selectedCosmetic;
        //

        // }
        //
        // public void OnClickBackButton()
        // {
        //     if (selectedCategory == AvatarUICategory.None)
        //         return;
        //     
        //     PopulateCategories();
        // }
        //
        // private void OnSelectedAvatarChanged(Avatar newAvatar)
        // {
        //     PopulateCategories();
        //     GroupCosmeticsByCategory();
        // }
        //
        // private void PopulateCategories()
        // {
        //     selectedCategory = AvatarUICategory.None;
        //     backButton.gameObject.SetActive(false);
        //     PoolPopulatedIcons();
        //     
        //     foreach (AvatarUICategory category in Enum.GetValues(typeof(AvatarUICategory)))
        //     {
        //         if (category == AvatarUICategory.None)
        //             continue;
        //         
        //         CategoryOptionUI categoryOption = categoryOptionPrefab.GetSpareOrCreate<CategoryOptionUI>(contentHolder);
        //         categoryOption.Label.text = category.ToString();
        //         categoryOption.transform.SetAsLastSibling();
        //
        //         categoryOption.Button.onClick.RemoveAllListeners();
        //         categoryOption.Button.onClick.AddListener(() => PopulateCosmetics(category));
        //     }
        //
        //     ResetScrollRect();
        // }
        //
        // private void PopulateCosmetics(AvatarUICategory category)
        // {
        //     selectedCategory = category;
        //     backButton.gameObject.SetActive(true);
        //     PoolPopulatedIcons();
        //
        //     if (!cosmeticsGrouped.ContainsKey(category))
        //         return;
        //     
        //     foreach (AvatarCosmetic cosmetic in cosmeticsGrouped[category])
        //     {
        //         CategoryOptionUI categoryOption = categoryOptionPrefab.GetSpareOrCreate<CategoryOptionUI>(contentHolder);
        //         categoryOption.Label.text = cosmetic.Name;
        //         categoryOption.transform.SetAsLastSibling();
        //
        //         categoryOption.Button.onClick.RemoveAllListeners();
        //         categoryOption.Button.onClick.AddListener(() => OnClickCosmeticOption(cosmetic));
        //     }
        //
        //     ResetScrollRect();
        // }
        //
        // private void OnClickCosmeticOption(AvatarCosmetic cosmetic)
        // {
        //     selectedCosmetic = cosmetic;
        // }
        //
        // private void ResetScrollRect()
        // {
        //     Canvas.ForceUpdateCanvases();
        //     scrollRect.horizontalNormalizedPosition = 0;
        // }
        //
        // private void PoolPopulatedIcons()
        // {
        //     foreach (Transform child in contentHolder)
        //         child.gameObject.Pool();
        // }
        //
        // private void DestroyPopulatedIcons()
        // {
        //     foreach (Transform child in contentHolder)
        //         Destroy(child.gameObject);
        // }
        //

    }
}
