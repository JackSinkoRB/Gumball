using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ColourOption : MonoBehaviour
    {

        public event Action onSelect;
        
        [SerializeField] private Image primaryColour;
        [SerializeField] private Image secondaryColour;

        private ColourSwatchSerialized swatch;
        
        public void Initialise(ColourSwatch swatch)
        {
            this.swatch = swatch.Serialize();
            
            primaryColour.color = swatch.Color.WithAlphaSetTo(1);
            secondaryColour.color = swatch.Specular.WithAlphaSetTo(1);
            
            onSelect = null;
        }
        
        public void Initialise(ColourSwatchSerialized swatch)
        {
            this.swatch = swatch;
            
            primaryColour.color = swatch.Color.ToColor().WithAlphaSetTo(1);
            secondaryColour.color = swatch.Specular.ToColor().WithAlphaSetTo(1);
            
            onSelect = null;
        }

        public void OnSelect()
        {
            onSelect?.Invoke();
        }
        
    }
}
