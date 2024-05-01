using System;
using System.Collections;
using System.Collections.Generic;
using HSVPicker;
using MagneticScrollUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class BodyPaintWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private GameObject simpleMenu;
        [SerializeField] private GameObject advancedMenu;

        [Header("Advanced")]
        [SerializeField] private ColorPicker primaryColorPicker;
        [SerializeField] private ColorPicker secondaryColorPicker;
        [SerializeField] private Slider metallicSlider;
        [SerializeField] private Slider glossSlider;
        [SerializeField] private Slider clearcoatSlider;
        
        [Header("Simple")]
        [SerializeField] private MagneticScroll swatchMagneticScroll;

        private readonly ColourSwatch advancedSwatch = new();
        private BodyPaintModification.PaintMode? currentMode;
        
        private BodyPaintModification paintModification => WarehouseManager.Instance.CurrentCar.BodyPaintModification;

        public override void Show()
        {
            base.Show();

            SelectTab(paintModification.CurrentPaintMode);
        }

        /// <summary>
        /// Workaround for unity events.
        /// </summary>
        public void SelectTab(int index) => SelectTab((BodyPaintModification.PaintMode)index);
        
        public void SelectTab(BodyPaintModification.PaintMode paintMode)
        {
            if (currentMode == paintMode)
                return; //already selected

            currentMode = paintMode;

            switch (paintMode)
            {
                case BodyPaintModification.PaintMode.SIMPLE:
                    OnSelectSimpleTab();
                    break;
                case BodyPaintModification.PaintMode.ADVANCED:
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
            advancedSwatch.SetColor(paintModification.CurrentSwatch.Color.ToColor());
            advancedSwatch.SetEmission(paintModification.CurrentSwatch.Emission.ToColor());
            advancedSwatch.SetMetallic(paintModification.CurrentSwatch.Metallic);
            advancedSwatch.SetSmoothness(paintModification.CurrentSwatch.Smoothness);
            advancedSwatch.SetClearcoat(paintModification.CurrentSwatch.ClearCoat);
            
            //update the UI
            primaryColorPicker.AssignColor(advancedSwatch.Color);
            secondaryColorPicker.AssignColor(advancedSwatch.Emission);
            metallicSlider.value = advancedSwatch.Metallic;
            glossSlider.value = advancedSwatch.Smoothness;
            clearcoatSlider.value = advancedSwatch.ClearCoat;

            simpleMenu.gameObject.SetActive(false);
            advancedMenu.gameObject.SetActive(true);
        }
        
        private void PopulateSwatchScroll(BodyPaintModification bodyPaintModification, bool showCustomSwatchOption)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            if (showCustomSwatchOption)
            {
                //add an additional ScrollItem at the beginning with the advanced colour settings
                ColourSwatchSerialized colourSwatch = bodyPaintModification.CurrentSwatch;
                ScrollItem customSwatchItem = new ScrollItem();
                
                customSwatchItem.onLoad += () =>
                {
                    ColourScrollIcon partsScrollIcon = (ColourScrollIcon)customSwatchItem.CurrentIcon;
                    partsScrollIcon.ImageComponent.color = colourSwatch.Color.ToColor();
                    partsScrollIcon.SecondaryColour.color = colourSwatch.Emission.ToColor();
                };

                customSwatchItem.onSelect += () =>
                {
                    bodyPaintModification.ApplySwatch(colourSwatch);
                };
                
                scrollItems.Add(customSwatchItem);
            }

            for (int index = 0; index < GlobalPaintPresets.Instance.BodySwatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.BodySwatchPresets[index];
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
                    bodyPaintModification.ApplySwatch(colourSwatch);
                    bodyPaintModification.CurrentSelectedPresetIndex = finalIndex;
                };

                scrollItems.Add(scrollItem);
            }

            int indexToShow = showCustomSwatchOption ? 0 : bodyPaintModification.CurrentSelectedPresetIndex;
            swatchMagneticScroll.SetItems(scrollItems, indexToShow);
        }
        
    }
}
