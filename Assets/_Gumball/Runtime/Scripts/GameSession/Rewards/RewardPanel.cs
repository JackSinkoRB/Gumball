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
        [SerializeField, ReadOnly] private List<SubPart> rewardQueueSubParts = new();

        public int PendingRewards => rewardQueueCoreParts.Count + rewardQueueSubParts.Count;
        
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
        
        public void QueueReward(SubPart subPart)
        {
            if (rewardQueueSubParts.Contains(subPart))
            {
                Debug.LogWarning($"Tried queuing {subPart.name} but it is already queued.");
                return;
            }
            
            rewardQueueSubParts.Add(subPart);
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

            //show sub parts
            if (rewardQueueSubParts.Count > 0)
            {
                SubPart subPart = rewardQueueSubParts[0];
                rewardQueueSubParts.RemoveAt(0);
                
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    RewardScrollIcon rewardScrollIcon = (RewardScrollIcon) scrollItem.CurrentIcon;
                    rewardScrollIcon.Initialise(subPart.DisplayName, subPart.Icon);
                };
                
                scrollItems.Add(scrollItem);
            }
            
            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
