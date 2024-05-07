using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class SwapCorePartPanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll partsMagneticScroll;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart.PartType partType;
        
        public void Initialise(CorePart.PartType type)
        {
            
        }
        
    }
}
