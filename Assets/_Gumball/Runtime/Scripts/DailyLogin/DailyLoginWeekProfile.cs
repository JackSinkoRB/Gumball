using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DailyLoginWeekProfile
    {

        [SerializeField] private MinorDailyLoginReward day1Reward;
        [SerializeField] private MinorDailyLoginReward day2Reward;
        [SerializeField] private MinorDailyLoginReward day3Reward;
        [SerializeField] private MinorDailyLoginReward day4Reward;
        [SerializeField] private MinorDailyLoginReward day5Reward;
        [SerializeField] private MinorDailyLoginReward day6Reward;
        [SerializeField] private MajorDailyLoginReward day7Reward;

        public MinorDailyLoginReward[] MinorRewards => new[] { day1Reward, day2Reward, day3Reward, day4Reward, day5Reward, day6Reward };
        public MajorDailyLoginReward MajorReward => day7Reward;

    }
}
