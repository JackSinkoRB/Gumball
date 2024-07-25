using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WeeklyChallengesSubMenu : ChallengesSubMenu
    {

        [SerializeField] private ChallengeUI challengeUIPrefab;
        [SerializeField] private Transform challengeUIHolder;

        public override void Show()
        {
            base.Show();

            foreach (Transform child in challengeUIHolder)
                child.gameObject.Pool();
            
            for (int slotIndex = 0; slotIndex < ChallengeManager.Instance.Weekly.NumberOfChallenges; slotIndex++)
            {
                Challenge currentChallenge = ChallengeManager.Instance.Weekly.GetCurrentChallenge(slotIndex);
                if (currentChallenge == null)
                    continue;

                ChallengeUI challengeUI = challengeUIPrefab.gameObject.GetSpareOrCreate<ChallengeUI>(challengeUIHolder);
                challengeUI.transform.SetAsLastSibling();
                challengeUI.Initialise(currentChallenge);
            }
        }
        
    }
}
