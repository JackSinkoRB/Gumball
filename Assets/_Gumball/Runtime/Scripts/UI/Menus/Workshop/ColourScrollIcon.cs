using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ColourScrollIcon : ScrollIcon
    {

        [SerializeField] private Image secondaryColour;

        public Image SecondaryColour => secondaryColour;
        
    }
}
