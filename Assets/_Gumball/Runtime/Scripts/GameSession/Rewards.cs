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
        [SerializeField] private BlueprintReward[] blueprints = Array.Empty<BlueprintReward>();
        
        public int XP => xp;
        public int StandardCurrency => standardCurrency;
        public CorePart[] CoreParts => coreParts ?? Array.Empty<CorePart>();
        public SubPart[] SubParts => subParts ?? Array.Empty<SubPart>();
        public bool FuelRefill => fuelRefill;
        public int PremiumCurrency => premiumCurrency;
        public Unlockable[] Unlockables => unlockables ?? Array.Empty<Unlockable>();
        public BlueprintReward[] Blueprints => blueprints ?? Array.Empty<BlueprintReward>();

        public Rewards(int xp = 0, int standardCurrency = 0, int premiumCurrency = 0, bool fuelRefill = false, CorePart[] coreParts = null, SubPart[] subParts = null, Unlockable[] unlockables = null, BlueprintReward[] blueprints = null)
        {
            this.xp = xp;
            this.standardCurrency = standardCurrency;
            this.premiumCurrency = premiumCurrency;
            this.fuelRefill = fuelRefill;
            this.coreParts = coreParts ?? Array.Empty<CorePart>();
            this.subParts = subParts ?? Array.Empty<SubPart>();
            this.unlockables = unlockables ?? Array.Empty<Unlockable>();
            this.blueprints = blueprints ?? Array.Empty<BlueprintReward>();
        }
        
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
        
        public IEnumerator GiveRewards(bool showUI = true)
        {
            //give XP
            if (xp > 0)
            {
                int previousLevelIndex = ExperienceManager.Level - 1;
                ExperienceManager.AddXP(xp); //add XP after in case there's a level up
                int currentLevelIndex = ExperienceManager.Level - 1;
                
                //give level up rewards
                for (int levelIndexToReward = previousLevelIndex + 1; levelIndexToReward <= currentLevelIndex; levelIndexToReward++)
                {
                    PlayerLevel level = ExperienceManager.GetLevelFromIndex(levelIndexToReward);
                    yield return level.Rewards.GiveRewards(false);
                }
            }

            //give standard currency
            if (standardCurrency > 0)
                Currency.Standard.AddFunds(standardCurrency);

            //give premium currency
            if (premiumCurrency > 0)
                Currency.Premium.AddFunds(premiumCurrency);

            //replenish fuel
            if (fuelRefill)
                FuelManager.Instance.ReplenishFuel();
            
            //give core parts
            foreach (CorePart corePartReward in CoreParts)
            {
                if (!corePartReward.IsUnlocked)
                    corePartReward.SetUnlocked(true);
            }

            //give sub parts
            foreach (SubPart subPartReward in SubParts)
            {
                if (!subPartReward.IsUnlocked)
                    subPartReward.SetUnlocked(true);
            }
            
            //unlock unlockables
            foreach (Unlockable unlockable in Unlockables)
                unlockable.Unlock();

            //give blueprints
            foreach (BlueprintReward blueprintReward in Blueprints)
                blueprintReward.GiveReward();
            
            //show the reward panel with queued rewards
            if (showUI)
                yield return ShowQueuedRewardUI();
        }

        /// <summary>
        /// The rewards can be shown if they have been previously queued but the UI hasn't been shown yet.
        /// </summary>
        public IEnumerator ShowQueuedRewardUI()
        {
            bool startedShowingVignette = false;
            if ((PanelManager.PanelExists<VignetteBackgroundPanel>() && !PanelManager.GetPanel<VignetteBackgroundPanel>().IsShowing) 
                && (!PanelManager.PanelExists<EndOfSessionVignetteBackgroundPanel>() || !PanelManager.GetPanel<EndOfSessionVignetteBackgroundPanel>().IsShowing))
            {
                PanelManager.GetPanel<VignetteBackgroundPanel>().Show();
                startedShowingVignette = true;
            }

            //do XP
            if (xp > 0 && PanelManager.PanelExists<XPGainedPanel>())
            {
                int previousXP = ExperienceManager.TotalXP - xp;
                int currentXP = ExperienceManager.TotalXP;
                PanelManager.GetPanel<XPGainedPanel>().Initialise(previousXP, currentXP);
                PanelManager.GetPanel<XPGainedPanel>().Show();

                yield return new WaitUntil(() => !PanelManager.PanelExists<XPGainedPanel>() || (!PanelManager.GetPanel<XPGainedPanel>().IsShowing && !PanelManager.GetPanel<XPGainedPanel>().IsTransitioning));

                int previousLevelIndex = ExperienceManager.GetLevelIndexFromTotalXP(previousXP);
                int currentLevelIndex = ExperienceManager.GetLevelIndexFromTotalXP(currentXP);

                bool leveledUp = previousLevelIndex < currentLevelIndex;
                if (leveledUp)
                {
                    //show the level up panel with the rewards (just for the last level gained)
                    if (PanelManager.PanelExists<LevelUpPanel>())
                    {
                        PanelManager.GetPanel<LevelUpPanel>().Show();
                
                        //populate level up panel with the rewards
                        PanelManager.GetPanel<LevelUpPanel>().Populate(ExperienceManager.GetLevelFromIndex(currentLevelIndex));

                        yield return new WaitUntil(() => !PanelManager.GetPanel<LevelUpPanel>().IsShowing && !PanelManager.GetPanel<LevelUpPanel>().IsTransitioning);
                    }

                    //show rewards
                    for (int levelIndexToReward = previousLevelIndex + 1; levelIndexToReward <= currentLevelIndex; levelIndexToReward++)
                    {
                        PlayerLevel level = ExperienceManager.GetLevelFromIndex(levelIndexToReward);
                        yield return level.Rewards.ShowQueuedRewardUI();
                    }
                }
            }
            
            //do unlocks
            if (PanelManager.PanelExists<UnlockableAnnouncementPanel>())
            {
                foreach (Unlockable unlockable in unlockables)
                {
                    PanelManager.GetPanel<UnlockableAnnouncementPanel>().Show();
                    PanelManager.GetPanel<UnlockableAnnouncementPanel>().Populate(unlockable);

                    yield return new WaitUntil(() => !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsShowing && !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsTransitioning);
                }
            }

            //do rewards
            if (PanelManager.PanelExists<RewardPanel>())
            {
                PanelManager.GetPanel<RewardPanel>().Initialise(this);
                if (PanelManager.GetPanel<RewardPanel>().HasRewards)
                    PanelManager.GetPanel<RewardPanel>().Show();

                yield return new WaitUntil(() => !PanelManager.PanelExists<RewardPanel>() || !PanelManager.GetPanel<RewardPanel>().IsShowing);
            }
            
            if (startedShowingVignette && PanelManager.PanelExists<VignetteBackgroundPanel>())
            {
                if (PanelManager.PanelExists<VignetteBackgroundPanel>())
                    PanelManager.GetPanel<VignetteBackgroundPanel>().Hide();
                if (PanelManager.PanelExists<EndOfSessionVignetteBackgroundPanel>())
                    PanelManager.GetPanel<EndOfSessionVignetteBackgroundPanel>().Hide();
            }
        }

    }
}
