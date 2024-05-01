using System;
using System.Collections;
using System.Collections.Generic;
using HSVPicker;
using MagneticScrollUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PaintWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private GameObject simpleMenu;
        [SerializeField] private GameObject advancedMenu;

        [SerializeField] private ColorPicker primaryColorPicker;
        [SerializeField] private ColorPicker secondaryColorPicker;
        [SerializeField] private Slider metallicSlider;
        [SerializeField] private Slider glossSlider;
        [SerializeField] private Slider clearcoatSlider;
        
        [SerializeField] private MagneticScroll bodyColourSwatchMagneticScroll;

        private readonly ColourSwatch advancedSwatch = new();
        private PaintModification.PaintMode? currentMode;
        
        private PaintModification paintModification => WarehouseManager.Instance.CurrentCar.PaintModification;

        public override void Show()
        {
            base.Show();

            SelectTab(paintModification.CurrentBodyPaintMode);
        }

        /// <summary>
        /// Workaround for unity events.
        /// </summary>
        public void SelectTab(int index) => SelectTab((PaintModification.PaintMode)index);
        
        public void SelectTab(PaintModification.PaintMode paintMode)
        {
            if (currentMode == paintMode)
                return; //already selected

            currentMode = paintMode;

            switch (paintMode)
            {
                case PaintModification.PaintMode.SIMPLE:
                    OnSelectSimpleTab();
                    break;
                case PaintModification.PaintMode.ADVANCED:
                    OnSelectAdvancedTab();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(paintMode), paintMode, null);
            }
        }

        public void OnPrimaryColourChange()
        {
            advancedSwatch.SetColor(primaryColorPicker.CurrentColor);
            paintModification.ApplySwatch(advancedSwatch);
        }
        
        public void OnSecondaryColourChange()
        {
            advancedSwatch.SetEmission(secondaryColorPicker.CurrentColor);
            paintModification.ApplySwatch(advancedSwatch);
        }

        public void OnMetallicSliderChange()
        {
            advancedSwatch.SetMetallic(metallicSlider.value);
            paintModification.ApplySwatch(advancedSwatch);
        }
        
        public void OnGlossSliderChange()
        {
            advancedSwatch.SetSmoothness(glossSlider.value);
            paintModification.ApplySwatch(advancedSwatch);
        }
        
        public void OnClearcoatSliderChange()
        {
            advancedSwatch.SetClearcoat(clearcoatSlider.value);
            paintModification.ApplySwatch(advancedSwatch);
        }
        
        private void OnSelectSimpleTab()
        {
            bool isCustomSwatch = paintModification.GetCurrentSwatchIndexInPresets() == -1;
            PopulateSwatchScroll(paintModification, isCustomSwatch);
            
            simpleMenu.gameObject.SetActive(true);
            advancedMenu.gameObject.SetActive(false);
        }
        
        private void OnSelectAdvancedTab()
        {
            advancedSwatch.SetColor(paintModification.CurrentBodyColour.Color.ToColor());
            advancedSwatch.SetEmission(paintModification.CurrentBodyColour.Emission.ToColor());
            advancedSwatch.SetMetallic(paintModification.CurrentBodyColour.Metallic);
            advancedSwatch.SetSmoothness(paintModification.CurrentBodyColour.Smoothness);
            advancedSwatch.SetClearcoat(paintModification.CurrentBodyColour.ClearCoat);
            
            //update the UI
            primaryColorPicker.AssignColor(advancedSwatch.Color);
            secondaryColorPicker.AssignColor(advancedSwatch.Emission);
            metallicSlider.value = advancedSwatch.Metallic;
            glossSlider.value = advancedSwatch.Smoothness;
            clearcoatSlider.value = advancedSwatch.ClearCoat;

            simpleMenu.gameObject.SetActive(false);
            advancedMenu.gameObject.SetActive(true);
        }
        
        private void PopulateSwatchScroll(PaintModification paintModification, bool showCustomSwatchOption)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            if (showCustomSwatchOption)
            {
                //add an additional ScrollItem at the beginning with the advanced colour settings
                ColourSwatchSerialized colourSwatch = paintModification.CurrentBodyColour;
                ScrollItem customSwatchItem = new ScrollItem();
                
                customSwatchItem.onLoad += () =>
                {
                    ColourScrollIcon partsScrollIcon = (ColourScrollIcon)customSwatchItem.CurrentIcon;
                    partsScrollIcon.ImageComponent.color = colourSwatch.Color.ToColor();
                    partsScrollIcon.SecondaryColour.color = colourSwatch.Emission.ToColor();
                };

                customSwatchItem.onSelect += () =>
                {
                    paintModification.ApplySwatch(colourSwatch);
                };
                
                scrollItems.Add(customSwatchItem);
            }

            for (int index = 0; index < paintModification.SwatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = paintModification.SwatchPresets[index];
                ScrollItem scrollItem = new ScrollItem();
                int finalIndex = index;
                
                scrollItem.onLoad += () =>
                {
                    ColourScrollIcon partsScrollIcon = (ColourScrollIcon)scrollItem.CurrentIcon;
                    partsScrollIcon.ImageComponent.color = colourSwatch.Color;
                    partsScrollIcon.SecondaryColour.color = colourSwatch.Emission;
                };

                scrollItem.onSelect += () =>
                {
                    paintModification.ApplySwatch(colourSwatch);
                    paintModification.CurrentSelectedPresetIndex = finalIndex;
                };

                scrollItems.Add(scrollItem);
            }

            int indexToShow = showCustomSwatchOption ? 0 : paintModification.CurrentSelectedPresetIndex;
            bodyColourSwatchMagneticScroll.SetItems(scrollItems, indexToShow);
        }
        
    }
}
