using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarColourPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll magneticScroll;
        [SerializeField] private Sprite circleIcon;
        
        public void Populate(ItemCosmetic itemCosmetic)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            if (itemCosmetic != null)
            {
                ItemCosmetic.Colorable colorable = itemCosmetic.CurrentItemData.Colorable;
                
                for (int index = 0; index < colorable.Colors.Length; index++)
                {
                    ScrollItem scrollItem = new ScrollItem();

                    int finalIndex = index;
                    scrollItem.onLoad += () =>
                    {
                        scrollItem.CurrentIcon.ImageComponent.enabled = true;
                        scrollItem.CurrentIcon.ImageComponent.sprite = circleIcon;
                        scrollItem.CurrentIcon.ImageComponent.color = colorable.Colors[finalIndex];
                    };
                    scrollItem.onSelect += () =>
                    {
                        itemCosmetic.ApplyColor(finalIndex);
                    };

                    scrollItems.Add(scrollItem);
                }
            }
            
            magneticScroll.SetItems(scrollItems, itemCosmetic.CurrentColorIndex);
        }
        
    }
}
