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

        public DailyLoginMonthProfile[] MonthProfiles => monthProfiles;

        public long SecondsPassedInCurrentMonth => PlayFabManager.CurrentEpochSecondsSynced % SecondsPerMonth;
        public int DaysPassedInCurrentMonth => Mathf.FloorToInt((float)SecondsPassedInCurrentMonth / TimeUtils.SecondsInADay);
        public int DaysRemainingInCurrentMonth => DaysInMonth - DaysPassedInCurrentMonth;
        public long SecondsLeftInCurrentDay => ((DaysPassedInCurrentMonth + 1) * TimeUtils.SecondsInADay) - SecondsPassedInCurrentMonth;

        public int CurrentMonthIndex => Mathf.FloorToInt((float)PlayFabManager.CurrentEpochSecondsSynced / SecondsPerMonth) % monthProfiles.Length;
        public DailyLoginMonthProfile CurrentMonth => monthProfiles[CurrentMonthIndex];
        
    }
}