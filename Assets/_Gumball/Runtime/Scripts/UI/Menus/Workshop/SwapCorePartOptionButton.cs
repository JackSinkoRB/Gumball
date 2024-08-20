using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwapCorePartOptionButton : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject frame;
        [Space(5)]
        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode glowCurrentCarColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowSelectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowDeselectedColor;

        [Header("Debugging")]
        [SerializeField] private CorePart corePart;
        
        private bool IsCurrentPart => corePart == CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarIndex, corePart.Type);

        public void Initialise(CorePart corePart)
        {
            this.corePart = corePart;
            
            title.text = corePart == null ? "Stock" : corePart.DisplayName;
            
            icon.sprite = corePart == null ? null : corePart.Icon;
            icon.gameObject.SetActive(icon.sprite != null);
            
            OnDeselect();
        }

        public void OnClickButton()
        {
            
        }
        
        public void OnSelect()
        {
            frame.SetActive(true);
            
            if (!IsCurrentPart)
                glow.color = GlobalColourPalette.Instance.GetGlobalColor(glowSelectedColor);
        }

        public void OnDeselect()
        {
            frame.SetActive(false);
            
            glow.color = GlobalColourPalette.Instance.GetGlobalColor(IsCurrentPart ? glowCurrentCarColor : glowDeselectedColor);
        }
        
    }
}
