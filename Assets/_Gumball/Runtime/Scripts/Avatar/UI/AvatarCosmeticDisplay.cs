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
                    int finalIndex = index;

                    cosmetic.OnCreateScrollItem(scrollItem, finalIndex);
                    scrollItem.onSelect += () => cosmetic.Apply(finalIndex);

                    scrollItems.Add(scrollItem);
                }
            }

            magneticScroll.SetItems(scrollItems, cosmetic.CurrentIndex);
        }
        
    }
}
