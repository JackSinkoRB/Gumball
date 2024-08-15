using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class CarOptionUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject frame;
        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode glowCurrentCarColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowSelectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowDeselectedColor;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private WarehouseCarData carData;
        [SerializeField, ReadOnly] private int carIndex;

        public WarehouseCarData CarData => carData;
        public int CarIndex => carIndex;

        public bool IsCurrentCar => carIndex == WarehouseManager.Instance.SavedCarIndex;
        
        public void Initialise(WarehouseCarData carData, int carIndex)
        {
            this.carData = carData;
            this.carIndex = carIndex;
            
            nameLabel.text = carData.DisplayName;
            
            icon.sprite = carData.Icon;
            icon.gameObject.SetActive(icon.sprite != null);
            OnDeselect();
        }

        public void OnClickButton()
        {
            PanelManager.GetPanel<SwapCarPanel>().SelectCarOption(this);
        }
        
        public void OnSelect()
        {
            frame.SetActive(true);
            
            if (!IsCurrentCar)
                glow.color = GlobalColourPalette.Instance.GetGlobalColor(glowSelectedColor);
        }

        public void OnDeselect()
        {
            frame.SetActive(false);
            
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(IsCurrentCar ? glowCurrentCarColor : glowDeselectedColor);
        }

    }
}
