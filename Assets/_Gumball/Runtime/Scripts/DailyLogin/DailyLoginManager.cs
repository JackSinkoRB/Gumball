using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Daily Login Manager")]
    public class DailyLoginManager : SingletonScriptable<DailyLoginManager>
    {
        
#region STATIC
        public delegate void CurrentMonthChangeDelegate(int previousMonthIndex, int newMonthIndex);
        public static event CurrentMonthChangeDelegate onCurrentMonthChange;

        private static int lastTrackedMonthIndex = -1;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            CoroutineHelper.onUnityLateUpdate -= Update;
            CoroutineHelper.onUnityLateUpdate += Update;
        }

        private static void Update()
        {
            CheckForMonthChange();
        }

        private static void CheckForMonthChange()
        {
            if (!HasLoaded)
                return;

            if (PlayFabManager.ServerTimeInitialisationStatus != PlayFabManager.ConnectionStatusType.SUCCESS) //requires internet
                return;
            
            bool isFirstCall = lastTrackedMonthIndex == -1;
            if (isFirstCall)
            {
                lastTrackedMonthIndex = Instance.CurrentMonthIndex;
                return;
            }

            if (lastTrackedMonthIndex != Instance.CurrentMonthIndex)
            {
                onCurrentMonthChange?.Invoke(lastTrackedMonthIndex, Instance.CurrentMonthIndex);
                lastTrackedMonthIndex = Instance.CurrentMonthIndex;
            }
        }
#endregion

        public const int DaysInWeek = 7;
        public const int WeeksInMonth = 4;
        public const int DaysInMonth = DaysInWeek * WeeksInMonth;
        public const long SecondsPerMonth = DaysInMonth * TimeUtils.SecondsInADay;
        
        [SerializeField] private DailyLoginMonthProfile[] monthProfiles;

        private int CurrentMonthIndexTracker
        {
            get => DataManager.Player.Get("Challenges.CurrentMonthIndexTracker", -1);
            set => DataManager.Player.Set("Challenges.CurrentMonthIndexTracker", value);
        }
        
        private int CurrentDayNumberTracker
        {
            get => DataManager.Player.Get("Challenges.CurrentDayNumberTracker", 1);
            set => DataManager.Player.Set("Challenges.CurrentDayNumberTracker", value);
        }
        
        private long TimeWhenCurrentDayIsReady
        {
            get => DataManager.Player.Get("Challenges.TimeWhenCurrentDayIsReady", 0);
            set => DataManager.Player.Set("Challenges.TimeWhenCurrentDayIsReady", value);
        }
        
        public DailyLoginMonthProfile[] MonthProfiles => monthProfiles;

        public long SecondsPassedInCurrentMonth => PlayFabManager.CurrentEpochSecondsSynced % SecondsPerMonth;
        public int DaysPassedInCurrentMonth => Mathf.FloorToInt((float)SecondsPassedInCurrentMonth / TimeUtils.SecondsInADay);
        public int DaysRemainingInCurrentMonth => DaysInMonth - DaysPassedInCurrentMonth;
        public long SecondsLeftInCurrentDay => ((DaysPassedInCurrentMonth + 1) * TimeUtils.SecondsInADay) - SecondsPassedInCurrentMonth;
        
        public int CurrentMonthIndex => Mathf.FloorToInt((float)PlayFabManager.CurrentEpochSecondsSynced / SecondsPerMonth) % monthProfiles.Length;
        public DailyLoginMonthProfile CurrentMonth => monthProfiles[CurrentMonthIndex];

        public int GetCurrentDayNumber()
        {
            EnsureServerTimeIsSynced();
            CheckToResetDataIfMonthChanged();
            
            return CurrentDayNumberTracker;
        }
        
        public bool IsDayReady(int dayNumber)
        {
            EnsureServerTimeIsSynced();
            CheckToResetDataIfMonthChanged();

            if (dayNumber != CurrentDayNumberTracker)
                return false;
            
            return PlayFabManager.CurrentEpochSecondsSynced >= TimeWhenCurrentDayIsReady;
        }

        public bool IsDayClaimed(int dayNumber)
        {
            EnsureServerTimeIsSynced();
            CheckToResetDataIfMonthChanged();

            //the day has been claimed if the current day is greater than it
            return CurrentDayNumberTracker > dayNumber;
        }
        
        public void IncreaseCurrentDayNumber()
        {
            EnsureServerTimeIsSynced();
            
            CurrentDayNumberTracker++;
            TimeWhenCurrentDayIsReady = PlayFabManager.CurrentEpochSecondsSynced + SecondsLeftInCurrentDay;
        }

        private void EnsureServerTimeIsSynced()
        {
            if (PlayFabManager.ServerTimeInitialisationStatus != PlayFabManager.ConnectionStatusType.SUCCESS)
                throw new InvalidOperationException("Trying to access daily login, although the server time hasn't been synced.");
        }
        
        private void CheckToResetDataIfMonthChanged()
        {
            //save the current month index to save data, if it is different, reset the days save data
            if (CurrentMonthIndexTracker == CurrentMonthIndex)
                return;
            
            //reset the data
            CurrentMonthIndexTracker = CurrentMonthIndex;
            CurrentDayNumberTracker = 1;
            TimeWhenCurrentDayIsReady = 0;
            
            GlobalLoggers.ChallengesLogger.Log($"Detected month change - Current month index is {CurrentMonthIndex}");
        }
        
    }
}