using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class RewardManager
    {
        
        public static void GiveReward(CorePart corePart)
        {
            corePart.SetUnlocked(true);
            if (PanelManager.PanelExists<RewardPanel>())
                PanelManager.GetPanel<RewardPanel>().QueueReward(corePart);
        }
        
        public static void GiveReward(SubPart subPart)
        {
            subPart.SetUnlocked(true);
            if (PanelManager.PanelExists<RewardPanel>())
                PanelManager.GetPanel<RewardPanel>().QueueReward(subPart);
        }

        public static void GiveStandardCurrency(int amount)
        {
            Currency.Standard.AddFunds(amount);
            if (PanelManager.PanelExists<RewardPanel>())
                PanelManager.GetPanel<RewardPanel>().QueueStandardCurrencyReward(amount);
        }
        
        public static void GivePremiumCurrency(int amount)
        {
            Currency.Premium.AddFunds(amount);
            if (PanelManager.PanelExists<RewardPanel>())
                PanelManager.GetPanel<RewardPanel>().QueuePremiumCurrencyReward(amount);
        }
        
    }
}
