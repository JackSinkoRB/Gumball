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
        public Unlockable[] Unlockables => unlockables;

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

        public void GiveRewards()
        {
            //give premium currency
            if (premiumCurrencyReward > 0)
                Currency.Premium.AddFunds(premiumCurrencyReward);
            
            //replenish fuel
            if (fuelRefillReward)
                FuelManager.Instance.ReplenishFuel();
            
            //unlock unlockables
            foreach (Unlockable unlockable in unlockables)
                unlockable.Unlock();
        }

    }
}
