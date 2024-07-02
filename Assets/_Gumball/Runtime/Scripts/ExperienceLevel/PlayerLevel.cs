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

        [Header("Unlocks")]
        [SerializeField] private Unlockable[] unlockables;
        
        public int XPRequired => xpRequired;
        public bool FuelRefillReward => fuelRefillReward;
        public int PremiumCurrencyReward => premiumCurrencyReward;
        public Unlockable[] Unlockables => unlockables;

        public void GiveRewards()
        {
            //TODO: refill fuel
            //TODO: give premium currency
            
            //TODO: update unit tests
            
            UnlockUnlockables();
        }

        private void UnlockUnlockables()
        {
            foreach (Unlockable unlockable in unlockables)
            {
                unlockable.Unlock();
            }
        }
        
    }
}
