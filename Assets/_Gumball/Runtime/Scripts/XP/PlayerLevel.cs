using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PlayerLevel
    {
    
        [Tooltip("The XP required from the previous level to reach this level.")]
        [SerializeField, PositiveValueOnly] private int xpRequired;
        
        [Header("Rewards")]
        [SerializeField] private bool fuelRefillReward = true;
        [SerializeField, PositiveValueOnly] private int premiumCurrencyReward;
        [SerializeField] private Unlockable[] unlockables;
        
        public int XPRequired => xpRequired;
        public bool FuelRefillReward => fuelRefillReward;
        public int PremiumCurrencyReward => premiumCurrencyReward;

#if UNITY_EDITOR
        public void SetPremiumCurrencyReward(int amount)
        {
            premiumCurrencyReward = amount;
        }

        public void SetFuelRefillReward(bool refill)
        {
            fuelRefillReward = refill;
        }
#endif

        public IEnumerator GiveRewards()
        {
            //give premium currency
            if (premiumCurrencyReward > 0)
                Currency.Premium.AddFunds(premiumCurrencyReward);
            
            if (fuelRefillReward)
                FuelManager.ReplenishFuel();

            //show the level up panel with the rewards
            if (PanelManager.PanelExists<LevelUpPanel>())
            {
                PanelManager.GetPanel<LevelUpPanel>().Show();
                
                //populate level up panel with the rewards
                PanelManager.GetPanel<LevelUpPanel>().Populate(this);

                yield return new WaitUntil(() => !PanelManager.GetPanel<LevelUpPanel>().IsShowing && !PanelManager.GetPanel<LevelUpPanel>().IsTransitioning);
            }
            
            foreach (Unlockable unlockable in unlockables)
            {
                unlockable.Unlock();
                
                if (PanelManager.PanelExists<UnlockableAnnouncementPanel>())
                    yield return new WaitUntil(() => !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsShowing && !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsTransitioning);
            }
        }

    }
}
