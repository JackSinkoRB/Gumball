using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(MultiImageButton))]
    public class PaintMaterialButton : MonoBehaviour
    {

        private bool isInitialised;
        private bool isSelected;
        private readonly Dictionary<Graphic, Color> defaultGraphicColors = new();
        
        public MultiImageButton Button => GetComponent<MultiImageButton>();
        
        private void Initialise()
        {
            isInitialised = true;

            foreach (Graphic graphic in Button.TargetGraphics)
                defaultGraphicColors[graphic] = graphic.color;
        }

        public void Select()
        {
            if (!isInitialised)
                Initialise();
            
            isSelected = true;
            
            foreach (Graphic graphic in Button.TargetGraphics)
            {
                Color selectedColor = defaultGraphicColors[graphic];
                graphic.color = selectedColor;
            }
        }

        public void Deselect()
        {
            if (!isInitialised)
                Initialise();
            
            isSelected = false;
            
            foreach (Graphic graphic in Button.TargetGraphics)
            {
                const float deselectedDarkeningPercent = 0.5f;
                Color selectedColor = defaultGraphicColors[graphic];
                Color deselectedColor = new Color(selectedColor.r - deselectedDarkeningPercent, selectedColor.g - deselectedDarkeningPercent, selectedColor.b - deselectedDarkeningPercent);
                graphic.color = deselectedColor;
            }
        }
        
    }
}
