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

        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private MagneticScroll magneticScroll;

        private AvatarColourPanel colourPanel => PanelManager.GetPanel<AvatarEditorPanel>().ColourPanel;

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

                    scrollItems.Add(scrollItem);
                }
            }

            int startIndex = cosmetic == null ? 0 : cosmetic.CurrentIndex == -1 ? cosmetic.GetSavedIndex() : cosmetic.CurrentIndex;
            magneticScroll.SetItems(scrollItems, startIndex);

            if (cosmetic.IsColorable)
            {
                colourPanel.Show();
                colourPanel.Populate(cosmetic);
            }
            else
            {
                PanelManager.GetPanel<AvatarEditorPanel>().ColourPanel.Hide();
            }
        }
        
    }
}
