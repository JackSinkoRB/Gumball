using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class AvatarCosmeticPanel : AnimatedPanel
    {

        [SerializeField] private AvatarColourPanel colourPanel;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private MagneticScroll magneticScroll;

        public AvatarCosmetic SelectedCosmetic { get; private set; }

        protected override void OnShow()
        {
            base.OnShow();

            CheckToShowColourPanel(SelectedCosmetic);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            SelectedCosmetic = null;
            colourPanel.Hide();
        }

        public void PopulateCosmeticOptions(AvatarCosmetic cosmetic)
        {
            SelectedCosmetic = cosmetic;
            
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
