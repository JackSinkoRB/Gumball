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
            UpdateColors();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColors();
        }
#endif

        private void UpdateColors()
        {
            var globalColour = GlobalColourPalette.Instance.GetGlobalColor(colourCode).WithAlphaSetTo(image.color.a);
        
            if (image != null)
                image.color = globalColour;

            if (label != null)
                label.color = globalColour;
        }
        
    }
}
