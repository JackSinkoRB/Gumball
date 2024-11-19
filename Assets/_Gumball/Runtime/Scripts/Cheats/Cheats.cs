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

        public static bool AllSessionsAreUnlocked
        {
            get => DataManager.Settings.Get("Cheats.AllSessionsAreUnlocked", false);
            set => DataManager.Settings.Set("Cheats.AllSessionsAreUnlocked", value);
        }

        public void ResetGame()
        {
            DataManager.RemoveAllData();

            if (Application.isPlaying)
                StartCoroutine(GameReloadManager.ReloadGame());
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
            foreach (WarehouseCarData carData in WarehouseManager.Instance.AllCarData)
                BlueprintManager.Instance.AddBlueprints(carData.GUID, BlueprintManager.Instance.Levels[^1].BlueprintsRequired);
        }
        
        public void UnlockAllSessions()
        {
            AllSessionsAreUnlocked = true;
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