using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class DailyLoginNodeColourCodes
    {
        
        [SerializeField] private GlobalColourPalette.ColourCode locked;
        [SerializeField] private GlobalColourPalette.ColourCode unlocked;
        [SerializeField] private GlobalColourPalette.ColourCode claimed;
            
        public Color LockedColor => GlobalColourPalette.Instance.GetGlobalColor(locked);
        public Color UnlockedColor => GlobalColourPalette.Instance.GetGlobalColor(unlocked);
        public Color ClaimedColor => GlobalColourPalette.Instance.GetGlobalColor(claimed);
        
    }
}
