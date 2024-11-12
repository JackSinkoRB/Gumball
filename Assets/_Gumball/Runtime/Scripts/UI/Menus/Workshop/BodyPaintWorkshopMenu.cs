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
    public class BodyPaintWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private GameObject simpleMenu;
        [SerializeField] private GameObject advancedMenu;
        [SerializeField] private SwitchButton switchButton;

        [Header("Advanced")]
        [SerializeField] private ColorPicker primaryColorPicker;
        [SerializeField] private ColorPicker secondaryColorPicker;
        [SerializeField] private Slider glossSlider;
        [SerializeField] private Slider clearcoatSlider;
        
        [Header("Simple")]
        [SerializeField] private ColourOption colourOptionPrefab;
        [SerializeField] private GridLayoutWithScreenSize colourOptionHolder;
        [SerializeField] private PaintMaterialButton[] paintMaterialButtons;
        
        private readonly ColourSwatch advancedSwatch = new();
        private BodyPaintModification.PaintMode? currentMode;
        
        private BodyPaintModification paintModification => WarehouseManager.Instance.CurrentCar.BodyPaintModification;

        protected override void OnShow()
        {
            base.OnShow();

            SelectTab(paintModification.CurrentPaintMode);
        }

        protected override void OnHide()
        {
            base.OnHide();

            currentMode = null;
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

        public void SetPaintMaterialType(int materialTypeIndex)
        {
            //functionality:
            paintModification.SetMaterialType((PaintMaterial.Type)materialTypeIndex);
            
            //UI selection:
            for (int index = 0; index < paintMaterialButtons.Length; index++)
            {
                PaintMaterialButton button = paintMaterialButtons[index];
                
                bool isSelected = (index+1) == materialTypeIndex; //add 1 to account for NONE
                if (isSelected)
                    button.Select();
                else
                    button.Deselect();
            }
        }

        public void OnPrimaryColourChange()
        {
            advancedSwatch.SetColor(primaryColorPicker.CurrentColor);
            paintModification.ApplySwatch(advancedSwatch);
        }
        
        public void OnSecondaryColourChange()
        {
            advancedSwatch.SetSpecular(secondaryColorPicker.CurrentColor);
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
            switchButton.OnClickLeftSwitch();
            
            bool isCustomSwatch = paintModification.GetCurrentSwatchIndexInPresets() == -1;
            PopulateSwatchScroll(isCustomSwatch);
            SetPaintMaterialType((int)paintModification.SavedSwatch.MaterialType);
            
            simpleMenu.gameObject.SetActive(true);
            advancedMenu.gameObject.SetActive(false);
        }
        
        private void OnSelectAdvancedTab()
        {
            switchButton.OnClickRightSwitch();

            advancedSwatch.SetColor(paintModification.SavedSwatch.Color.ToColor());
            advancedSwatch.SetSpecular(paintModification.SavedSwatch.Specular.ToColor());
            advancedSwatch.SetSmoothness(paintModification.SavedSwatch.Smoothness);
            advancedSwatch.SetClearcoat(paintModification.SavedSwatch.ClearCoat);
            
            //update the UI
            primaryColorPicker.AssignColor(advancedSwatch.Color);
            secondaryColorPicker.AssignColor(advancedSwatch.Specular);
            glossSlider.value = advancedSwatch.Smoothness;
            clearcoatSlider.value = advancedSwatch.ClearCoat;

            simpleMenu.gameObject.SetActive(false);
            advancedMenu.gameObject.SetActive(true);
        }
        
        private void PopulateSwatchScroll(bool showCustomSwatchOption)
        {
            foreach (Transform child in colourOptionHolder.transform)
                child.gameObject.Pool();
            
            if (showCustomSwatchOption)
            {
                //add an additional ScrollItem at the beginning with the advanced colour settings
                
                ColourSwatchSerialized colourSwatch = paintModification.SavedSwatch;
                
                ColourOption instance = colourOptionPrefab.gameObject.GetSpareOrCreate<ColourOption>(colourOptionHolder.transform);
                instance.Initialise(colourSwatch);
                instance.onSelect += OnSelectInstance;
                instance.transform.SetAsLastSibling();

                void OnSelectInstance()
                {
                    paintModification.ApplySwatch(colourSwatch);
                    SetPaintMaterialType((int)colourSwatch.MaterialType);
                }
            }

            for (int index = 0; index < GlobalPaintPresets.Instance.BodySwatchPresets.Length; index++)
            {
                int finalIndex = index;
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.BodySwatchPresets[index];
                ColourOption instance = colourOptionPrefab.gameObject.GetSpareOrCreate<ColourOption>(colourOptionHolder.transform);
                instance.Initialise(colourSwatch);
                instance.onSelect += OnSelectInstance;
                instance.transform.SetAsLastSibling();

                void OnSelectInstance()
                {
                    paintModification.ApplySwatch(colourSwatch);
                    paintModification.SavedSelectedPresetIndex = finalIndex;
                    SetPaintMaterialType((int)colourSwatch.MaterialType);
                }
            }
            
            this.PerformAtEndOfFrame(() => colourOptionHolder.Resize());
        }
        
    }
}
