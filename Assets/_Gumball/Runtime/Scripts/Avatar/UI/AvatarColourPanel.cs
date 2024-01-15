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
        
        public void Populate(AvatarCosmetic cosmetic)
        {
            if (!cosmetic.IsColorable)
                return;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            if (cosmetic != null)
            {
                for (int index = 0; index <= cosmetic.GetMaxColorIndex(); index++)
                {
                    ScrollItem scrollItem = new ScrollItem();

                    int finalIndex = index;
                    scrollItem.onLoad += () =>
                    {
                        scrollItem.CurrentIcon.ImageComponent.sprite = circleIcon;
                        scrollItem.CurrentIcon.ImageComponent.color = cosmetic.Colors[finalIndex];
                    };
                    scrollItem.onSelect += () =>
                    {
                        cosmetic.ApplyColor(finalIndex);
                    };

                    scrollItems.Add(scrollItem);
                }
            }

            int startIndex = cosmetic == null ? 0 : cosmetic.CurrentColorIndex == -1 ? cosmetic.GetSavedColourIndex() : cosmetic.CurrentColorIndex;
            magneticScroll.SetItems(scrollItems, startIndex);
        }
        
    }
}
