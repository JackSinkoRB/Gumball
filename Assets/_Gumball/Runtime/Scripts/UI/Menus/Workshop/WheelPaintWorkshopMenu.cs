using System;
using System.Collections;
using System.Collections.Generic;
using HSVPicker;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class WheelPaintWorkshopMenu : WorkshopSubMenu
    {
        
        public enum WheelsToModifyPosition
        {
            ALL,
            FRONT,
            REAR
        }
        
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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private WheelsToModifyPosition wheelsToModifyPosition;

        private readonly ColourSwatch advancedSwatch = new();
        private WheelPaintModification.PaintMode? currentMode;
        
        private WheelPaintModification selectedWheelPaintModification => wheelsToModify[0].GetComponent<WheelPaintModification>();
        private WheelMesh[] wheelsToModify => wheelsToModifyPosition == WheelsToModifyPosition.ALL ? WarehouseManager.Instance.CurrentCar.AllWheelMeshes
            : (wheelsToModifyPosition == WheelsToModifyPosition.FRONT ?
                WarehouseManager.Instance.CurrentCar.FrontWheelMeshes : WarehouseManager.Instance.CurrentCar.RearWheelMeshes);
        
        public override void Show()
        {
            base.Show();
            
            SetWheelsToModifyPosition(WheelsToModifyPosition.ALL);
            
            SelectTab(selectedWheelPaintModification.CurrentPaintMode);
        }

        /// <summary>
        /// Workaround for unity events.
        /// </summary>
        public void SelectTab(int index) => SelectTab((WheelPaintModification.PaintMode)index);
        
        public void SelectTab(WheelPaintModification.PaintMode paintMode)
        {
            if (currentMode == paintMode)
                return; //already selected

            currentMode = paintMode;

            switch (paintMode)
            {
                case WheelPaintModification.PaintMode.SIMPLE:
                    OnSelectSimpleTab();
                    break;
                case WheelPaintModification.PaintMode.ADVANCED:
                    OnSelectAdvancedTab();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(paintMode), paintMode, null);
            }
        }
        
        /// <summary>
        /// Workaround for unity events.
        /// </summary>
        public void SetWheelsToModifyPosition(int index) => SetWheelsToModifyPosition((WheelsToModifyPosition)index);
        
        /// <summary>
        /// Set which wheels will be modified when using the sliders.
        /// </summary>
        public void SetWheelsToModifyPosition(WheelsToModifyPosition position)
        {
            wheelsToModifyPosition = position;
            
            UpdateInputUI();
        }

        public void OnPrimaryColourChange()
        {
            advancedSwatch.SetColor(primaryColorPicker.CurrentColor);
            ApplyAdvancedSwatchToSelectedWheels();
        }

        public void OnSecondaryColourChange()
        {
            advancedSwatch.SetEmission(secondaryColorPicker.CurrentColor);
            ApplyAdvancedSwatchToSelectedWheels();
        }

        public void OnMetallicSliderChange()
        {
            advancedSwatch.SetMetallic(metallicSlider.value);
            ApplyAdvancedSwatchToSelectedWheels();
        }
        
        public void OnGlossSliderChange()
        {
            advancedSwatch.SetSmoothness(glossSlider.value);
            ApplyAdvancedSwatchToSelectedWheels();
        }
        
        public void OnClearcoatSliderChange()
        {
            advancedSwatch.SetClearcoat(clearcoatSlider.value);
            ApplyAdvancedSwatchToSelectedWheels();
        }

        private void ApplyAdvancedSwatchToSelectedWheels()
        {
            foreach (WheelMesh wheelMesh in wheelsToModify)
            {
                WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                paintModification.ApplySwatch(advancedSwatch);
            }
        }
        
        /// <summary>
        /// Updates the slider values with the cars current values.
        /// </summary>
        private void UpdateInputUI()
        {
            //read from the actual material
            advancedSwatch.SetColor(selectedWheelPaintModification.CurrentSwatch.Color.ToColor());
            advancedSwatch.SetEmission(selectedWheelPaintModification.CurrentSwatch.Emission.ToColor());
            advancedSwatch.SetMetallic(selectedWheelPaintModification.CurrentSwatch.Metallic);
            advancedSwatch.SetSmoothness(selectedWheelPaintModification.CurrentSwatch.Smoothness);
            advancedSwatch.SetClearcoat(selectedWheelPaintModification.CurrentSwatch.ClearCoat);
            
            primaryColorPicker.AssignColor(advancedSwatch.Color);
            secondaryColorPicker.AssignColor(advancedSwatch.Emission);

            metallicSlider.value = advancedSwatch.Metallic;
            glossSlider.value = advancedSwatch.Smoothness;
            clearcoatSlider.value = advancedSwatch.ClearCoat;
        }

        private void OnSelectSimpleTab()
        {
            bool isCustomSwatch = selectedWheelPaintModification.GetCurrentSwatchIndexInPresets() == -1;
            PopulateSwatchScroll(isCustomSwatch);
            
            simpleMenu.gameObject.SetActive(true);
            advancedMenu.gameObject.SetActive(false);
        }
        
        private void OnSelectAdvancedTab()
        {
            simpleMenu.gameObject.SetActive(false);
            advancedMenu.gameObject.SetActive(true);
        }
        
        private void PopulateSwatchScroll(bool showCustomSwatchOption)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            
            if (showCustomSwatchOption)
            {
                //add an additional ScrollItem at the beginning with the advanced colour settings
                ColourSwatchSerialized colourSwatch = selectedWheelPaintModification.CurrentSwatch;
                ScrollItem customSwatchItem = new ScrollItem();
                
                customSwatchItem.onLoad += () =>
                {
                    ColourScrollIcon partsScrollIcon = (ColourScrollIcon)customSwatchItem.CurrentIcon;
                    partsScrollIcon.ImageComponent.color = colourSwatch.Color.ToColor();
                    partsScrollIcon.SecondaryColour.color = colourSwatch.Emission.ToColor();
                };

                customSwatchItem.onSelect += () =>
                {
                    selectedWheelPaintModification.ApplySwatch(colourSwatch);
                };
                
                scrollItems.Add(customSwatchItem);
            }

            for (int index = 0; index < GlobalPaintPresets.Instance.WheelSwatchPresets.Length; index++)
            {
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.WheelSwatchPresets[index];
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
                    foreach (WheelMesh wheelMesh in wheelsToModify)
                    {
                        WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                        paintModification.ApplySwatch(colourSwatch);
                        paintModification.CurrentSelectedPresetIndex = finalIndex;
                    }
                };

                scrollItems.Add(scrollItem);
            }

            int indexToShow = showCustomSwatchOption ? 0 : selectedWheelPaintModification.CurrentSelectedPresetIndex;
            swatchMagneticScroll.SetItems(scrollItems, indexToShow);
        }
        
    }
}
