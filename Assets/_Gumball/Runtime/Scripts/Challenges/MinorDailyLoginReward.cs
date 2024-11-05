using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class MinorDailyLoginReward : DailyLoginReward
    {

        [SerializeField] private int standardCurrencyReward;

        public int StandardCurrencyReward => standardCurrencyReward;
        
    }
}
