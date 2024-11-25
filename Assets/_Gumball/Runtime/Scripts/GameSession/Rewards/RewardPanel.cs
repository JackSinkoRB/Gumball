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
        [SerializeField] private Sprite premiumCurrencyIcon;
        [SerializeField] private RewardUI rewardUIPrefab;
        [SerializeField] private Transform rewardsHolder;
        
        private Rewards rewards;

        public bool HasRewards => rewards.Blueprints.Length > 0 || rewards.CoreParts.Length > 0 || rewards.SubParts.Length > 0 || rewards.StandardCurrency > 0 || rewards.PremiumCurrency > 0;
        
        public void Initialise(Rewards rewards)
        {
            this.rewards = rewards;
            
            Populate();
        }

        private void Populate()
        {
            foreach (Transform child in rewardsHolder)
                child.gameObject.Pool();

            //show blueprints first
            foreach (BlueprintReward blueprintReward in rewards.Blueprints)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                WarehouseCarData carData = WarehouseManager.Instance.GetCarDataFromGUID(blueprintReward.CarGUID);
                instance.Initialise(carData.Icon, $"{carData.DisplayName} x{blueprintReward.Blueprints}");
            }

            //show core parts
            foreach (CorePart corePart in rewards.CoreParts)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                instance.Initialise(corePart.Icon, corePart.DisplayName);
            }

            //show sub parts
            foreach (SubPart subPart in rewards.SubParts)
            {
                RewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                instance.Initialise(subPart.Icon, subPart.DisplayName);
            }

            //show premium currency
            if (rewards.PremiumCurrency > 0)
            {
                RewardUI premiumCurrencyInstance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                premiumCurrencyInstance.Initialise(premiumCurrencyIcon, $"{rewards.PremiumCurrency}");
            }

            //show standard currency
            if (rewards.StandardCurrency > 0)
            {
                RewardUI standardCurrencyInstance = rewardUIPrefab.gameObject.GetSpareOrCreate<RewardUI>(rewardsHolder);
                standardCurrencyInstance.Initialise(standardCurrencyIcon, $"{rewards.StandardCurrency}");
            }
        }

    }
}