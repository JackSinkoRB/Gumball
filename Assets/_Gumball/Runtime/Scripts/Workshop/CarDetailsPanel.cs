using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class CarDetailsPanel : AnimatedPanel
    {

        [SerializeField] private Material blackUnlitMaterial;
        [SerializeField] private TextMeshProUGUI makeLabel;
        [SerializeField] private TextMeshProUGUI modelLabel;

        [SerializeField, ReadOnly] private CarOptionUI selectedOption;

        public void Initialise(CarOptionUI selectedOption)
        {
            this.selectedOption = selectedOption;

            makeLabel.text = selectedOption.CarData.MakeDisplayName;
            modelLabel.text = selectedOption.CarData.DisplayName;

            //typeLabel.text = selectedOption.CarData.CarType.ToString();
            
            if (selectedOption.CarData.IsUnlocked)
        }

        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<SwapCarPanel>().Show();
        }
        
        public void OnClickSelectButton()
        {
            Hide();
            
            PanelManager.GetPanel<WarehousePanel>().Show();
            PanelManager.GetPanel<PlayerStatsPanel>().Show();

            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SwapCurrentCar(selectedOption.CarIndex));
        }

    }
}
