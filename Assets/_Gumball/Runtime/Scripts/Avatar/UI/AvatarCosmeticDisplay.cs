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
        
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private MagneticScroll magneticScroll;

        private AvatarColourPanel colourPanel => PanelManager.GetPanel<AvatarEditorPanel>().ColourPanel;

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
            if (cosmetic is ItemCosmetic itemCosmetic
                && itemCosmetic.CurrentItemData.Colorable.IsColorable
                && itemCosmetic.CurrentItemData.Colorable.Colors.Length > 1) //require at least 2 colors for it to show
            {
                colourPanel.Show();
                colourPanel.Populate(itemCosmetic);
            }
            else
            {
                colourPanel.Hide();
            }
        }
        
    }
}
