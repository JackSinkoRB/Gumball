using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class MajorDailyLoginReward : DailyLoginReward
    {

        [SerializeField] private Rewards rewards;

        public Rewards Rewards => rewards;
        
    }
}
