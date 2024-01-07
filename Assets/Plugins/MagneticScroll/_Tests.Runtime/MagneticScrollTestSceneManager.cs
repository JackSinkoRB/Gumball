using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagneticScrollUtils.Tests.Runtime
{
    public class MagneticScrollTestSceneManager : MonoBehaviour
    {

        [Header("Populated")]
        [SerializeField] private MagneticScroll populatedMagneticScroll;
        [SerializeField] private Color[] populateColors;
        
        [Header("Pre-defined")]
        [SerializeField] private MagneticScroll predefinedMagneticScroll;

        public MagneticScroll PopulatedMagneticScroll => populatedMagneticScroll;
        public Color[] PopulateColors => populateColors;
        
        public MagneticScroll PredefinedMagneticScroll => predefinedMagneticScroll;
        
    }
}
