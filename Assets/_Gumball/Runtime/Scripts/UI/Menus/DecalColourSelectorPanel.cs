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

        private bool hasLoggedColorChangeSinceSelectingDecal;

        public MagneticScroll MagneticScroll => magneticScroll;
        
        private void OnEnable()
        {
            DecalEditor.onSelectLiveDecal += OnSelectLiveDecal;
        }
        
        private void OnDisable()
        {
            DecalEditor.onSelectLiveDecal -= OnSelectLiveDecal;
        }

        private void OnSelectLiveDecal(LiveDecal liveDecal)
        {
            hasLoggedColorChangeSinceSelectingDecal = false;
        }

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
                    if (liveDecal.ColorIndex == finalIndex)
                        return; //already this colour

                    if (!hasLoggedColorChangeSinceSelectingDecal || DecalStateManager.NextUndoState is not DecalStateManager.ColorStateChange)
                    {
                        hasLoggedColorChangeSinceSelectingDecal = true;
                        DecalStateManager.LogStateChange(new DecalStateManager.ColorStateChange(liveDecal));
                    }

                    liveDecal.SetColorFromIndex(finalIndex);
                };

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems, liveDecal.ColorIndex);
        }
        
    }
}
