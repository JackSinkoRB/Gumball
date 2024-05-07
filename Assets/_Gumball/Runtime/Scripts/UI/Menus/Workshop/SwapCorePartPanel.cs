using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SwapCorePartPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll partsMagneticScroll;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        
        public void Initialise(CorePart.PartType type)
        {
            partType = type;
            
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            foreach (CorePart part in CorePartManager.GetSpareParts(type))
            {
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    CorePartScrollIcon scrollIcon = (CorePartScrollIcon) scrollItem.CurrentIcon;
                    scrollIcon.Initialise(part);
                };

                scrollItem.onSelect += () =>
                {
                    //TODO: populate details
                };
                
                scrollItems.Add(scrollItem);
            }
            
            partsMagneticScroll.SetItems(scrollItems);
        }
        
    }
}
