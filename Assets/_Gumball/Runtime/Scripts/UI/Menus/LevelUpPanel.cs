using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class LevelUpPanel : AnimatedPanel
    {

        public void Populate(int previousLevel, int newLevel)
        {
            for (int level = previousLevel + 1; level <= newLevel; level++)
            {
                //populate with the rewards
            }
        }
        
    }
}