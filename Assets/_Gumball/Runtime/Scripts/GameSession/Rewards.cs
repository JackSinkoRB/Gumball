using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class Rewards
    {

        [SerializeField, PositiveValueOnly] private int xp;
        [SerializeField, PositiveValueOnly] private int standardCurrency;
        [SerializeField, PositiveValueOnly] private int premiumCurrency;
        [SerializeField] private bool fuelRefill;
        [SerializeField, DisplayInspector] private CorePart[] coreParts = Array.Empty<CorePart>();
        [SerializeField, DisplayInspector] private SubPart[] subParts = Array.Empty<SubPart>();
        [SerializeField] private Unlockable[] unlockables = Array.Empty<Unlockable>();
        
        public int XP => xp;
        public int StandardCurrency => standardCurrency;
        public CorePart[] CoreParts => coreParts;
        public SubPart[] SubParts => subParts;
        public bool FuelRefill => fuelRefill;
        public int PremiumCurrency => premiumCurrency;
        public Unlockable[] Unlockables => unlockables;
        
#if UNITY_EDITOR
        public void SetPremiumCurrencyReward(int amount)
        {
            premiumCurrency = amount;
        }

        public void SetFuelRefillReward(bool refill)
        {
            fuelRefill = refill;
        }
#endif
        
        public IEnumerator GiveRewards()
        {
            //give XP
            if (xp > 0)
            {
                int currentXP = ExperienceManager.TotalXP;
                int newXP = ExperienceManager.TotalXP + xp;
                
                if (PanelManager.PanelExists<XPGainedPanel>())
                {
                    PanelManager.GetPanel<XPGainedPanel>().Show();
                    PanelManager.GetPanel<XPGainedPanel>().TweenExperienceBar(currentXP, newXP);
                    
                    yield return new WaitUntil(() => !PanelManager.PanelExists<XPGainedPanel>() || (!PanelManager.GetPanel<XPGainedPanel>().IsShowing && !PanelManager.GetPanel<XPGainedPanel>().IsTransitioning));
                }

                ExperienceManager.AddXP(xp); //add XP after in case there's a level up
                
                yield return new WaitUntil(() => (!PanelManager.PanelExists<LevelUpPanel>() || (!PanelManager.GetPanel<LevelUpPanel>().IsShowing && !PanelManager.GetPanel<LevelUpPanel>().IsTransitioning))
                                                 && (!PanelManager.PanelExists<UnlockableAnnouncementPanel>() || (!PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsShowing && !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsTransitioning)));
            }

            //give standard currency
            if (standardCurrency > 0)
                RewardManager.GiveStandardCurrency(standardCurrency);

            //give premium currency
            if (premiumCurrency > 0)
                Currency.Premium.AddFunds(premiumCurrency);
            
            //replenish fuel
            if (fuelRefill)
                FuelManager.Instance.ReplenishFuel();
            
            //give core parts
            if (coreParts != null)
            {
                foreach (CorePart corePartReward in coreParts)
                {
                    if (!corePartReward.IsUnlocked)
                        RewardManager.GiveReward(corePartReward);
                }
            }

            //give sub parts
            if (subParts != null)
            {
                foreach (SubPart subPartReward in subParts)
                {
                    if (!subPartReward.IsUnlocked)
                        RewardManager.GiveReward(subPartReward);
                }
            }
            
            //unlock unlockables
            foreach (Unlockable unlockable in unlockables)
                unlockable.Unlock();

            //show the reward panel with queued rewards
            if (PanelManager.PanelExists<RewardPanel>() && PanelManager.GetPanel<RewardPanel>().PendingRewards > 0)
            {
                PanelManager.GetPanel<RewardPanel>().Show();
                yield return new WaitUntil(() => !PanelManager.PanelExists<RewardPanel>() || (!PanelManager.GetPanel<RewardPanel>().IsShowing && !PanelManager.GetPanel<RewardPanel>().IsTransitioning));
            }
        }
        
    }
}
