using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarEditorCosmeticButton : MonoBehaviour
    {

        [SerializeField] private Image icon;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color deselectedColor = Color.grey;

        private AvatarCosmeticSelector selector;
        private AvatarCosmetic cosmetic;
        
        public void Initialise(AvatarCosmeticSelector selector, AvatarCosmetic cosmetic)
        {
            this.selector = selector;
            this.cosmetic = cosmetic;
            
            icon.sprite = cosmetic.Icon;
        }

        private void OnEnable()
        {
            AvatarCosmeticSelector.onSelectCosmetic += OnCosmeticSelected; 
        }
        
        private void OnDisable()
        {
            AvatarCosmeticSelector.onSelectCosmetic -= OnCosmeticSelected;
        }

        public void OnClickButton()
        {
            selector.SelectCosmetic(cosmetic);
        }
        
        private void OnCosmeticSelected(AvatarCosmetic selectedCosmetic)
        {
            icon.color = selectedCosmetic == cosmetic ? selectedColor : deselectedColor;
        }
        
    }
}
