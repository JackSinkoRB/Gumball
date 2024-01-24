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
        
        public void Populate(AvatarCosmetic cosmetic, ColorableCosmeticOption colorable)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            if (cosmetic != null)
            {
                for (int index = 0; index < colorable.Colors.Length; index++)
                {
                    ScrollItem scrollItem = new ScrollItem();

                    int finalIndex = index;
                    scrollItem.onLoad += () =>
                    {
                        scrollItem.CurrentIcon.ImageComponent.sprite = circleIcon;
                        scrollItem.CurrentIcon.ImageComponent.color = colorable.Colors[finalIndex];
                    };
                    scrollItem.onSelect += () =>
                    {
                        AvatarCosmetic.ApplyColorIndex(cosmetic, finalIndex);
                    };

                    scrollItems.Add(scrollItem);
                }
            }
            
            magneticScroll.SetItems(scrollItems, AvatarCosmetic.GetCurrentColorIndex(cosmetic));
        }
        
    }
}
