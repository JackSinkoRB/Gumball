using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public class Cheats : MonoBehaviour
    {
        
        public void ResetGame()
        {
            DataManager.RemoveAllData();

            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
        }
        
        public void Add10000StandardCurrency()
        {
            Currency.Standard.AddFunds(10000);
        }

        public void GiveMaxLevel()
        {
            ExperienceManager.SetTotalXP(int.MaxValue);
        }

        public void UnlockAllParts()
        {
            foreach (CorePart corePart in CorePartManager.AllParts)
                corePart.SetUnlocked(true);
            
            foreach (SubPart subPart in SubPartManager.AllParts)
                subPart.SetUnlocked(true);
        }

        public void GiveAllBlueprints()
        {
            for (int carIndex = 0; carIndex < WarehouseManager.Instance.AllCarData.Count; carIndex++)
            {
                BlueprintManager.Instance.AddBlueprints(carIndex, BlueprintManager.Instance.BlueprintsRequiredForEachLevel[^1]);
            }
        }
        
        public void CompleteCurrentChallenges()
        {
            foreach (ChallengeTracker tracker in ChallengeTrackerManager.Instance.Trackers)
            {
                tracker.Track(int.MaxValue);
            }
        }
        
        public void Skip1Day()
        {
            TimeUtils.AddTimeOffset(TimeSpan.FromDays(1));
        }

    }
}