using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PlayerLevel
    {
    
        [Tooltip("The XP required from the previous level to reach this level.")]
        [SerializeField, PositiveValueOnly] private int xpRequired;
        [SerializeField] private Rewards rewards;

        public int XPRequired => xpRequired;
        public Rewards Rewards => rewards;

    }
}
