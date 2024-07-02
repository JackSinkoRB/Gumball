using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PlayerLevel
    {
    
        [Tooltip("The XP required from the previous level to reach this level.")]
        [SerializeField] private int xpRequired;

        public int XPRequired => xpRequired;

    }
}
