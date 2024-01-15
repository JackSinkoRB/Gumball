using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class AvatarCosmeticDisplay : MonoBehaviour
    {
        
        [SerializeField] private MagneticScroll magneticScroll;

        public void PopulateCosmeticOptions(AvatarCosmetic cosmetic)
        {
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
        }
        
    }
}
