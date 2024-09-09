using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class RewardPanel : AnimatedPanel
    {
        
        [SerializeField] private Sprite standardCurrencyIcon;
        [SerializeField] private RewardUI rewardUIPrefab;
        [SerializeField] private Transform rewardsHolder;
        [SerializeField] private Transform retryButton;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private List<CorePart> rewardQueueCoreParts = new();
        [SerializeField, ReadOnly] private List<SubPart> rewardQueueSubParts = new();
        [SerializeField, ReadOnly] private List<int> rewardQueueStandardCurrency = new();
        
        public int PendingRewards => rewardQueueCoreParts.Count + rewardQueueSubParts.Count + rewardQueueStandardCurrency.Count;
        
        protected override void OnShow()
        {
            base.OnShow();

            ShowRetryButton(false);
            Populate();
        }

        public void ShowRetryButton(bool show)
        {
            retryButton.gameObject.SetActive(show);
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
            foreach (Transform child in rewardsHolder)
                child.gameObject.Pool();
            
            //do core parts first
            foreach (CorePart corePart in rewardQueueCoreParts)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                instance.Initialise(corePart.Icon, corePart.DisplayName);
            }
            rewardQueueCoreParts.Clear();

            //show sub parts
            foreach (SubPart subPart in rewardQueueSubParts)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                instance.Initialise(subPart.Icon, subPart.DisplayName);
            }
            rewardQueueSubParts.Clear();
            
            //show standard currency
            foreach (int amount in rewardQueueStandardCurrency)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                instance.Initialise(standardCurrencyIcon, $"{amount}");
            }
            rewardQueueStandardCurrency.Clear();
        }
        
        public void OnClickRetryButton()
        {
            GameSessionManager.Instance.RestartCurrentSession();
        }
        
    }
}
