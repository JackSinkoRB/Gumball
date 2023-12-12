using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class DecalColourSelectorPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll magneticScroll;

        public MagneticScroll MagneticScroll => magneticScroll;
        
    }
}
