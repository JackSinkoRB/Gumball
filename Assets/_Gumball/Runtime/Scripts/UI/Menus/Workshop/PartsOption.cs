using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PartsOption : MonoBehaviour
    {

        private static Action<PartsOption> onSelectOption;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            onSelectOption = null;
        }

        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode selectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode deselectedColor;
        [SerializeField] private TextMeshProUGUI carNameLabel;
        [SerializeField] private TextMeshProUGUI partNameLabel;
        [SerializeField] private Image icon;
        [SerializeField] private Image keyIcon;

        private CarPart part;
        private CarPartGroup group;
        
        public void Initialise(CarPart part, CarPartGroup group)
        {
            this.part = part;
            this.group = group;
            
            carNameLabel.text = WarehouseManager.Instance.CurrentCar.DisplayName;
            partNameLabel.text = part.DisplayName;
            icon.sprite = part.Icon;
        }

        private void OnEnable()
        {
            onSelectOption += OnSelectPartsOption;
        }

        private void OnDisable()
        {
            onSelectOption -= OnSelectPartsOption;
        }

        public void OnSelect()
        {
            group.SetPartActive(part);
            
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(selectedColor);
            keyIcon.gameObject.SetActive(true);
            
            onSelectOption?.Invoke(this);
        }

        private void OnSelectPartsOption(PartsOption partsOption)
        {
            //check to deselect
            if (partsOption != this)
                OnDeselect();
        }
        
        private void OnDeselect()
        {
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(deselectedColor);
            keyIcon.gameObject.SetActive(false);
        }

    }
}
