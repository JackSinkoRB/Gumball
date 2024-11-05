using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DailyLoginMonthProfile
    {

        
        [SerializeField] private DailyLoginWeekProfile week1;
        [SerializeField] private DailyLoginWeekProfile week2;
        [SerializeField] private DailyLoginWeekProfile week3;
        [SerializeField] private DailyLoginWeekProfile week4;

        public DailyLoginWeekProfile[] Weeks => new[] { week1, week2, week3, week4 };

    }
}
