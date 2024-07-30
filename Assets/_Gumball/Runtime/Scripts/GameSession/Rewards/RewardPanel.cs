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
        [SerializeField] private Sprite standardCurrencyIcon;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<CorePart> rewardQueueCoreParts = new();
        [SerializeField, ReadOnly] private List<SubPart> rewardQueueSubParts = new();
        [SerializeField, ReadOnly] private List<int> rewardQueueStandardCurrency = new();
        
        public int PendingRewards => rewardQueueCoreParts.Count + rewardQueueSubParts.Count + rewardQueueStandardCurrency.Count;
        
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
        
        public void QueueStandardCurrencyReward(int amount)
        {
            rewardQueueStandardCurrency.Add(amount);
        }
        
        public void Populate()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            //do core parts first
            foreach (CorePart corePart in rewardQueueCoreParts)
            {
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    RewardScrollIcon rewardScrollIcon = (RewardScrollIcon) scrollItem.CurrentIcon;
                    rewardScrollIcon.Initialise(corePart.DisplayName, corePart.Icon);
                };
                
                scrollItems.Add(scrollItem);
            }
            rewardQueueCoreParts.Clear();

            //show sub parts
            foreach (SubPart subPart in rewardQueueSubParts)
            {
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    RewardScrollIcon rewardScrollIcon = (RewardScrollIcon) scrollItem.CurrentIcon;
                    rewardScrollIcon.Initialise(subPart.DisplayName, subPart.Icon);
                };
                
                scrollItems.Add(scrollItem);
            }
            rewardQueueSubParts.Clear();
            
            //show standard currency
            foreach (int amount in rewardQueueStandardCurrency)
            {
                ScrollItem scrollItem = new ScrollItem();

                scrollItem.onLoad += () =>
                {
                    RewardScrollIcon rewardScrollIcon = (RewardScrollIcon) scrollItem.CurrentIcon;
                    rewardScrollIcon.Initialise($"{amount}", standardCurrencyIcon);
                };

                scrollItems.Add(scrollItem);
            }
            rewardQueueStandardCurrency.Clear();

            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
