using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class RacerInfoProfile
    {
    
        [SerializeField] private string displayName = "Racer";
        [SerializeField] private Sprite icon;

        public Sprite Icon => icon;
        
    }
}
