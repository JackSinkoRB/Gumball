using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class RewardPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll magneticScroll;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<CorePart> rewardQueueCoreParts = new();

        public int PendingRewards => rewardQueueCoreParts.Count; //TODO: add other reward counts
        
        protected override void OnShow()
        {
            base.OnShow();
            
            Populate();
        }

        public void QueueReward(CorePart corePart)
        {
            if (rewardQueueCoreParts.Contains(corePart))
            {
                Debug.LogWarning($"Tried queuing {corePart.name} but it is already queued.");
                return;
            }
            
            rewardQueueCoreParts.Add(corePart);
        }

        public void Populate()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            //do core parts first
            if (rewardQueueCoreParts.Count > 0)
            {
                CorePart corePart = rewardQueueCoreParts[0];
                rewardQueueCoreParts.RemoveAt(0);
                
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    RewardScrollIcon rewardScrollIcon = (RewardScrollIcon) scrollItem.CurrentIcon;
                    rewardScrollIcon.Initialise(corePart.DisplayName, corePart.Icon);
                };
                
                scrollItems.Add(scrollItem);
            }

            //TODO: sub parts etc.
            
            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
