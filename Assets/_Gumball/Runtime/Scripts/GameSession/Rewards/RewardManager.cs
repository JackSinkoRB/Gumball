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
            PanelManager.GetPanel<RewardPanel>().QueueReward(corePart);
        }
        
        public static void GiveReward(SubPart subPart)
        {
            subPart.SetUnlocked(true);
            PanelManager.GetPanel<RewardPanel>().QueueReward(subPart);
        }
        
    }
}
