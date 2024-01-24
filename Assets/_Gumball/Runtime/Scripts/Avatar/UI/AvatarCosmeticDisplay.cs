using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class AvatarCosmeticDisplay : MonoBehaviour
    {

        public static event Action<AvatarCosmetic> onSelectCosmetic;
        
        [SerializeField] private AvatarColourPanel colourPanel;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private MagneticScroll magneticScroll;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            onSelectCosmetic = null;
        }
        
        public void PopulateCosmeticOptions(AvatarCosmetic cosmetic)
        {
            titleLabel.text = cosmetic.DisplayName;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            if (cosmetic != null)
            {
                for (int index = 0; index <= cosmetic.GetMaxIndex(); index++)
                {
                    ScrollItem scrollItem = new ScrollItem();
                    
                    cosmetic.OnCreateScrollItem(scrollItem, index);
                    
                    scrollItem.onSelect += () => CheckToShowColourPanel(cosmetic);
                    
                    scrollItems.Add(scrollItem);
                }
            }

            int startIndex = cosmetic == null ? 0 : cosmetic.CurrentIndex == -1 ? cosmetic.GetSavedIndex() : cosmetic.CurrentIndex;
            magneticScroll.SetItems(scrollItems, startIndex);

            CheckToShowColourPanel(cosmetic);
            
            onSelectCosmetic?.Invoke(cosmetic);
        }

        private void CheckToShowColourPanel(AvatarCosmetic cosmetic)
        {
            ColorableCosmeticOption? colorable = AvatarCosmetic.GetColorable(cosmetic);
            if (colorable != null)
            {
                colourPanel.Show();
                colourPanel.Populate(cosmetic, colorable.Value);
            }
            else
            {
                colourPanel.Hide();
            }
        }
        
    }
}
