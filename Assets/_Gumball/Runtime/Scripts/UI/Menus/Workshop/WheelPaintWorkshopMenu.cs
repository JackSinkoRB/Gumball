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
        [SerializeField] private SwitchButton switchButton;
        [Space(5)]
        [SerializeField] private WheelModificationPositionButton[] wheelModificationPositionButtons;
        
        [Header("Advanced")]
        [SerializeField] private ColorPicker primaryColorPicker;
        [SerializeField] private ColorPicker secondaryColorPicker;
        [SerializeField] private Slider glossSlider;
        [SerializeField] private Slider clearcoatSlider;
        
        [Header("Simple")]
        [SerializeField] private ColourOption colourOptionPrefab;
        [SerializeField] private GridLayoutWithScreenSize colourOptionHolder;
        [SerializeField] private PaintMaterialButton[] paintMaterialButtons;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private WheelsToModifyPosition wheelsToModifyPosition;

        private readonly ColourSwatch advancedSwatch = new();
        private WheelPaintModification.PaintMode? currentMode;
        
        private WheelMesh[] wheelsToModify => wheelsToModifyPosition == WheelsToModifyPosition.ALL ? WarehouseManager.Instance.CurrentCar.AllWheelMeshes
            : (wheelsToModifyPosition == WheelsToModifyPosition.FRONT ?
                WarehouseManager.Instance.CurrentCar.FrontWheelMeshes : WarehouseManager.Instance.CurrentCar.RearWheelMeshes);
        
        protected override void OnShow()
        {
            base.OnShow();
            
            SetWheelsToModifyPosition(WheelsToModifyPosition.ALL);
            
            SelectTab(wheelsToModify[0].GetComponent<WheelPaintModification>().CurrentPaintMode);
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
            
            for (int index = 0; index < wheelModificationPositionButtons.Length; index++)
            {
                WheelModificationPositionButton button = wheelModificationPositionButtons[index];
                WheelsToModifyPosition buttonPosition = (WheelsToModifyPosition)index;
                
                bool isSelected = buttonPosition == position;
                if (isSelected)
                    button.Select();
                else
                    button.Deselect();
            }
        }

        public void SetPaintMaterialType(int materialTypeIndex)
        {
            //functionality:
            foreach (WheelMesh wheelMesh in wheelsToModify)
            {
                wheelMesh.GetComponent<WheelPaintModification>().SetMaterialType((PaintMaterial.Type)materialTypeIndex);
            }

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
            ApplyAdvancedSwatchToSelectedWheels();
        }

        public void OnSecondaryColourChange()
        {
            advancedSwatch.SetSpecular(secondaryColorPicker.CurrentColor);
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
            WheelPaintModification selectedWheelPaintModification = wheelsToModify[0].GetComponent<WheelPaintModification>();
            advancedSwatch.SetColor(selectedWheelPaintModification.SavedSwatch.Color.ToColor());
            advancedSwatch.SetSpecular(selectedWheelPaintModification.SavedSwatch.Specular.ToColor());
            advancedSwatch.SetSmoothness(selectedWheelPaintModification.SavedSwatch.Smoothness);
            advancedSwatch.SetClearcoat(selectedWheelPaintModification.SavedSwatch.ClearCoat);
            
            primaryColorPicker.AssignColor(advancedSwatch.Color);
            secondaryColorPicker.AssignColor(advancedSwatch.Specular);

            glossSlider.value = advancedSwatch.Smoothness;
            clearcoatSlider.value = advancedSwatch.ClearCoat;
        }

        private void OnSelectSimpleTab()
        {
            switchButton.OnClickLeftSwitch();
            
            WheelPaintModification selectedWheelPaintModification = wheelsToModify[0].GetComponent<WheelPaintModification>();
            bool isCustomSwatch = selectedWheelPaintModification.GetSavedSwatchIndexInPresets() == -1;
            PopulateSwatchScroll(isCustomSwatch);
            
            SetPaintMaterialType((int)selectedWheelPaintModification.SavedSwatch.MaterialType);
            
            simpleMenu.gameObject.SetActive(true);
            advancedMenu.gameObject.SetActive(false);
        }
        
        private void OnSelectAdvancedTab()
        {
            switchButton.OnClickRightSwitch();
            
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
                
                WheelPaintModification selectedWheelPaintModification = wheelsToModify[0].GetComponent<WheelPaintModification>();
                ColourSwatchSerialized colourSwatch = selectedWheelPaintModification.SavedSwatch;
                
                ColourOption instance = colourOptionPrefab.gameObject.GetSpareOrCreate<ColourOption>(colourOptionHolder.transform);
                instance.Initialise(colourSwatch);
                instance.onSelect += OnSelectInstance;
                instance.transform.SetAsLastSibling();

                void OnSelectInstance()
                {
                    foreach (WheelMesh wheelMesh in wheelsToModify)
                    {
                        WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                        paintModification.ApplySwatch(colourSwatch);
                        SetPaintMaterialType((int)colourSwatch.MaterialType);
                    }
                }
            }

            for (int index = 0; index < GlobalPaintPresets.Instance.WheelSwatchPresets.Length; index++)
            {
                int finalIndex = index;
                ColourSwatch colourSwatch = GlobalPaintPresets.Instance.WheelSwatchPresets[index];
                ColourOption instance = colourOptionPrefab.gameObject.GetSpareOrCreate<ColourOption>(colourOptionHolder.transform);
                instance.Initialise(colourSwatch);
                instance.onSelect += OnSelectInstance;
                instance.transform.SetAsLastSibling();

                void OnSelectInstance()
                {
                    foreach (WheelMesh wheelMesh in wheelsToModify)
                    {
                        WheelPaintModification paintModification = wheelMesh.GetComponent<WheelPaintModification>();
                        paintModification.ApplySwatch(colourSwatch);
                        paintModification.SavedSelectedPresetIndex = finalIndex;
                        SetPaintMaterialType((int)colourSwatch.MaterialType);
                    }
                }
            }
            
            this.PerformAtEndOfFrame(() => colourOptionHolder.Resize());
        }
        
    }
}
