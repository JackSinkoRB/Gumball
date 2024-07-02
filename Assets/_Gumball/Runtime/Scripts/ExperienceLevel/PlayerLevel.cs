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

        //TODO: unit test to ensure all rewards are given and unlockables are unlocked, even in cases of levelling up multiple times at once (eg. level 1 to 3 should giev level 2 rewards too)
        public void GiveRewards()
        {
            //TODO: refill fuel
            //TODO: give premium currency
            
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
