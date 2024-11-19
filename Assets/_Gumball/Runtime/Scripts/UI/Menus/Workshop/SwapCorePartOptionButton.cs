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
        [SerializeField] private TextMeshProUGUI levelLabel;
        [Space(5)]
        [SerializeField] private Image glow;
        [SerializeField] private GlobalColourPalette.ColourCode glowCurrentCarColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowSelectedColor;
        [SerializeField] private GlobalColourPalette.ColourCode glowDeselectedColor;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private CorePart corePart;

        private Button button => GetComponent<Button>();

        public CorePart CorePart => corePart;
        public bool IsCurrentPart => corePart == CorePartManager.GetCorePart(WarehouseManager.Instance.CurrentCar.CarGUID, corePart.Type);

        public void Initialise(CorePart corePart)
        {
            this.corePart = corePart;
            
            title.text = corePart == null ? "Stock" : corePart.DisplayName;
            
            icon.sprite = corePart == null ? null : corePart.Icon;

            button.interactable = corePart != null && corePart.CarType == WarehouseManager.Instance.CurrentCar.CarType;

            int currentLevel = corePart.CurrentLevelIndex + 1;
            levelLabel.text = currentLevel.ToString();
                
            OnDeselect();
        }

        public void OnClickButton()
        {
            PanelManager.GetPanel<SwapCorePartPanel>().SelectPartOption(this);
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
