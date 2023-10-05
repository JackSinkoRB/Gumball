using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalTextureSelector : MonoBehaviour
    {

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject textureOptionPrefab;
        [SerializeField] private Transform contentHolder;
        [SerializeField] private Scrollbar horizontalScrollBar;
        
        private void OnEnable()
        {
            Populate();
        }

        private void Populate()
        {
            foreach (Sprite texture in DecalManager.Instance.TextureOptions)
            {
                TextureOptionUI textureOption = Instantiate(textureOptionPrefab, contentHolder).GetComponent<TextureOptionUI>();
                textureOption.TextureImage.sprite = texture;
            }
            
            horizontalScrollBar.value = 0;
            this.PerformAtEndOfFrame(() => scrollRect.enabled = horizontalScrollBar.gameObject.activeSelf);
        }
        
    }
}
