using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Daily Login Manager")]
    public class DailyLoginManager : SingletonScriptable<DailyLoginManager>
    {

        public const int DaysInWeek = 7;
        public const int WeeksInMonth = 4;
        public const int DaysInMonth = DaysInWeek * WeeksInMonth;
        public const long SecondsPerMonth = DaysInMonth * TimeUtils.SecondsInADay;

        [SerializeField] private DailyLoginMonthProfile[] monthProfiles;

        public DailyLoginMonthProfile[] MonthProfiles => monthProfiles;
        
        public DailyLoginMonthProfile CurrentMonth
        {
            get
            {
                int currentIndex = Mathf.FloorToInt((float)TimeUtils.CurrentEpochSeconds / SecondsPerMonth) % monthProfiles.Length;
                return monthProfiles[currentIndex];
            }
        }

    }
}