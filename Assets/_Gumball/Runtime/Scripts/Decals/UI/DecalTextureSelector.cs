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
                categoryOption.transform.SetAsLastSibling();

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
            foreach (DecalTexture decalTexture in selectedCategory.DecalTextures)
            {
                TextureOptionUI textureOption = textureOptionPrefab.GetSpareOrCreate<TextureOptionUI>(contentHolder);
                textureOption.Icon.sprite = decalTexture.Sprite;
                textureOption.transform.SetAsLastSibling();
                
                textureOption.Button.onClick.RemoveAllListeners();
                textureOption.Button.onClick.AddListener(() => OnClickTextureOption(decalTexture));
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
        
        private void OnClickTextureOption(DecalTexture decalTexture)
        {
            LiveDecal decal = DecalEditor.Instance.CreateLiveDecal(selectedCategory, decalTexture);
            DecalStateManager.LogStateChange(new DecalStateManager.CreateStateChange(decal));
            DecalEditor.Instance.SelectLiveDecal(decal);
        }

        private void ResetScrollRect()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.horizontalNormalizedPosition = 0;
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
