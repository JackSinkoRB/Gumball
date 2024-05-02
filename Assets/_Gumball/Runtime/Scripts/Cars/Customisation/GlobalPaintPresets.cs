using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Global Paint Presets")]
    public class GlobalPaintPresets : SingletonScriptable<GlobalPaintPresets>
    {
        
        [Header("Body")]
        [SerializeField] private Material defaultBodyMaterial;
        [SerializeField] private ColourSwatch[] bodySwatchPresets;
        
        [Header("Wheels")]
        [SerializeField] private Material defaultWheelMaterial;
        [SerializeField] private ColourSwatch[] wheelSwatchPresets;

        public Material DefaultBodyMaterial => defaultBodyMaterial;
        public ColourSwatch[] BodySwatchPresets => bodySwatchPresets;
        
        public Material DefaultWheelMaterial => defaultWheelMaterial;
        public ColourSwatch[] WheelSwatchPresets => wheelSwatchPresets;
        
    }
}
