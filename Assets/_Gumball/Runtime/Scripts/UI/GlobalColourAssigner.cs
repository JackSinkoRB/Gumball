using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class GlobalColourAssigner : MonoBehaviour
    {

        [SerializeField] private GlobalColourPalette.ColourCode colourCode;

        private Image image => GetComponent<Image>();
        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();

        private void OnEnable()
        {
            this.PerformAfterTrue(() => GlobalColourPalette.HasLoaded, UpdateColors);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColors();
        }
#endif

        public void SetColour(GlobalColourPalette.ColourCode colourCode)
        {
            this.colourCode = colourCode;
            UpdateColors();
        }

        private void UpdateColors()
        {
#if UNITY_EDITOR
            if (DataManager.IsUsingTestProviders) //disable if running tests as it causes issues
                return;
#endif
            
            Color globalColour = GlobalColourPalette.Instance.GetGlobalColor(colourCode);
        
            if (image != null)
                image.color = globalColour.WithAlphaSetTo(image.color.a);;

            if (label != null)
                label.color = globalColour.WithAlphaSetTo(label.color.a);;
        }
        
    }
}
