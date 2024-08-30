using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarEditorCosmeticButton : MonoBehaviour
    {

        [SerializeField] private Image icon;

        private AvatarCosmeticSelector selector;
        private AvatarCosmetic cosmetic;
        
        public void Initialise(AvatarCosmeticSelector selector, AvatarCosmetic cosmetic)
        {
            this.selector = selector;
            this.cosmetic = cosmetic;
            
            icon.sprite = cosmetic.Icon;
        }

        public void OnClickButton()
        {
            selector.SelectCosmetic(cosmetic);
        }
        
    }
}
