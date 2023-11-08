using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalTextureSelector : MonoBehaviour
    {

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject textureOptionPrefab;
        [SerializeField] private GameObject categoryOptionPrefab;
        [SerializeField] private Transform contentHolder;
        [SerializeField] private Scrollbar horizontalScrollBar;
        [SerializeField] private Button backButton;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private DecalUICategory selectedCategory;
        
        private void OnEnable()
        {
            PopulateCategories();
        }

        private void OnDisable()
        {
            selectedCategory = null;
            DestroyPopulatedIcons(); //don't keep textures in memory
        }

        public void OnClickBackButton()
        {
            if (selectedCategory == null)
                return;
            
            selectedCategory = null;
            PopulateCategories();
        }

        private void PopulateCategories()
        {
            backButton.gameObject.SetActive(false);
            
            PoolPopulatedIcons();
            foreach (DecalUICategory category in DecalManager.Instance.DecalUICategories)
            {
                CategoryOptionUI categoryOption = categoryOptionPrefab.GetSpareOrCreate<CategoryOptionUI>(contentHolder);
                categoryOption.Label.text = category.CategoryName;
                
                categoryOption.Button.onClick.RemoveAllListeners();
                categoryOption.Button.onClick.AddListener(() => OnClickCategoryOption(category));
            }

            ResetScrollRect();
        }

        /// <summary>
        /// Populates the texture in the current selected category.
        /// </summary>
        private void PopulateTextures()
        {
            backButton.gameObject.SetActive(true);

            PoolPopulatedIcons();
            foreach (Sprite sprite in selectedCategory.Sprites)
            {
                TextureOptionUI textureOption = textureOptionPrefab.GetSpareOrCreate<TextureOptionUI>(contentHolder);
                textureOption.Icon.sprite = sprite;
                
                textureOption.Button.onClick.RemoveAllListeners();
                textureOption.Button.onClick.AddListener(() => OnClickTextureOption(sprite));
            }

            ResetScrollRect();
        }

        private void OnClickCategoryOption(DecalUICategory category)
        {
            if (selectedCategory == category)
                return;
            
            selectedCategory = category;
            PopulateTextures();
        }
        
        private void OnClickTextureOption(Sprite sprite)
        {
            LiveDecal decal = DecalManager.Instance.CreateLiveDecal(selectedCategory, sprite);
            DecalManager.Instance.SelectLiveDecal(decal);
        }

        private void ResetScrollRect()
        {
            scrollRect.enabled = true;
            Canvas.ForceUpdateCanvases();
            horizontalScrollBar.value = 0;
            this.PerformAtEndOfFrame(() =>
            {
                if (!horizontalScrollBar.gameObject.activeSelf)
                    scrollRect.enabled = false;
            });
        }
        
        private void PoolPopulatedIcons()
        {
            foreach (Transform child in contentHolder)
                child.gameObject.Pool();
        }
        
        private void DestroyPopulatedIcons()
        {
            foreach (Transform child in contentHolder)
                Destroy(child.gameObject);
        }
        
    }
}
