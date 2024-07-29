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

        [SerializeField, PositiveValueOnly] public int xp;
        [SerializeField, PositiveValueOnly] public int standardCurrency;
        [SerializeField, DisplayInspector] public CorePart[] coreParts = Array.Empty<CorePart>();
        [SerializeField, DisplayInspector] public SubPart[] subParts = Array.Empty<SubPart>();

        public int XP => xp;
        public int StandardCurrency => standardCurrency;
        public CorePart[] CoreParts => coreParts;
        public SubPart[] SubParts => subParts;
        
        public IEnumerator GiveRewards()
        {
            //give XP
            if (xp > 0)
            {
                PanelManager.GetPanel<XPGainedPanel>().Show();
            
                int currentXP = ExperienceManager.TotalXP;
                int newXP = ExperienceManager.TotalXP + xp;
                
                PanelManager.GetPanel<XPGainedPanel>().TweenExperienceBar(currentXP, newXP);
                
                yield return new WaitUntil(() => !PanelManager.GetPanel<XPGainedPanel>().IsShowing && !PanelManager.GetPanel<XPGainedPanel>().IsTransitioning);
                
                ExperienceManager.AddXP(xp); //add XP after in case there's a level up
                
                yield return new WaitUntil(() => !PanelManager.GetPanel<LevelUpPanel>().IsShowing && !PanelManager.GetPanel<LevelUpPanel>().IsTransitioning &&
                                                 !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsShowing && !PanelManager.GetPanel<UnlockableAnnouncementPanel>().IsTransitioning);
            }

            //give standard currency
            if (standardCurrency > 0)
                RewardManager.GiveStandardCurrency(standardCurrency);

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
            
            //show the reward panel with queued rewards
            if (PanelManager.GetPanel<RewardPanel>().PendingRewards > 0)
            {
                PanelManager.GetPanel<RewardPanel>().Show();
                yield return new WaitUntil(() => !PanelManager.GetPanel<RewardPanel>().IsShowing && !PanelManager.GetPanel<RewardPanel>().IsTransitioning);
            }
        }
        
    }
}
