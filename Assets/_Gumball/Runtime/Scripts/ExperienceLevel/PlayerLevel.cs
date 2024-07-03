using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PlayerLevel
    {
    
        [Tooltip("The XP required from the previous level to reach this level.")]
        [SerializeField] private int xpRequired;
        
        [Header("Rewards")]
        [SerializeField] private bool fuelRefillReward = true;
        [SerializeField] private int premiumCurrencyReward;
        [SerializeField] private Unlockable[] unlockables;
        
        public int XPRequired => xpRequired;
        public bool FuelRefillReward => fuelRefillReward;
        public int PremiumCurrencyReward => premiumCurrencyReward;
        
        public IEnumerator GiveRewards()
        {
            //TODO: refill fuel
            //TODO: give premium currency
            
            //TODO: update unit tests
            
            //show the level up panel with the rewards
            PanelManager.GetPanel<LevelUpPanel>().Show();
            //TODO: populate level up panel with the rewards

            yield return new WaitUntil(() => !PanelManager.GetPanel<LevelUpPanel>().IsShowing && !PanelManager.GetPanel<LevelUpPanel>().IsTransitioning);
            
            foreach (Unlockable unlockable in unlockables)
            {
                unlockable.Unlock();
                
                yield return new WaitUntil(() => !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsShowing && !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsTransitioning);
            }
        }

    }
}
