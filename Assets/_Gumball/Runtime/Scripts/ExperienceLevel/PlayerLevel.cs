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
        
        public IEnumerator GiveRewards()
        {
            //give premium currency
            if (premiumCurrencyReward > 0)
                Currency.Premium.AddFunds(premiumCurrencyReward);
            
            //TODO: refill fuel

            //TODO: update unit tests
            
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
