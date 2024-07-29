using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WeeklyChallengesSubMenu : ChallengeChallengesSubMenu
    {

        protected override Challenges GetChallengeManager()
        {
            return ChallengeManager.Instance.Weekly;
        }
        
    }
}
