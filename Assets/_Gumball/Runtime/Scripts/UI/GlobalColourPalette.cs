using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Global Colour Palette")]
    public class GlobalColourPalette : SingletonScriptable<GlobalColourPalette>
    {

        public enum ColourCode
        {
            C1,
            C2,
            C3,
            C4,
            C5,
            C6,
            C7,
            C8,
            C9,
            C10,
            C11,
            C12,
            C13
        }

        [Serializable]
        public struct GlobalColour
        {
            [SerializeField] private ColourCode code;
            [SerializeField] private Color color;
        }

        [SerializeField] private GenericDictionary<ColourCode, Color> globalColours = new();

        public Color GetGlobalColor(ColourCode code)
        {
            return globalColours[code];
        }
        
    }
}
