using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DecalColourSelectorPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll magneticScroll;

        public MagneticScroll MagneticScroll => magneticScroll;

        public void Populate(LiveDecal liveDecal)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            for (int index = 0; index < DecalEditor.Instance.ColorPalette.Length; index++)
            {
                int finalIndex = index;
                Color color = DecalEditor.Instance.ColorPalette[index];
                
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.color = color;

                scrollItem.onSelect += () =>
                {
                    liveDecal.SetColorFromIndex(finalIndex);
                };

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems, liveDecal.ColorIndex);
        }
        
    }
}
