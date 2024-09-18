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

        [SerializeField] private TextMeshProUGUI makeLabel;
        [SerializeField] private TextMeshProUGUI modelLabel;
        [SerializeField] private Image carIcon;
        
        [SerializeField, ReadOnly] private CarOptionUI selectedOption;

        public void Initialise(CarOptionUI selectedOption)
        {
            this.selectedOption = selectedOption;

            makeLabel.text = selectedOption.CarData.MakeDisplayName;
            modelLabel.text = selectedOption.CarData.DisplayName;
            carIcon.sprite = selectedOption.CarData.Icon;
        }
        
        protected override void OnShow()
        {
            base.OnShow();

            PanelManager.GetPanel<PaintStripeBackgroundPanel>().Show();
        }
        
        protected override void OnHide()
        {
            base.OnHide();
            
            if (PanelManager.PanelExists<PaintStripeBackgroundPanel>())
                PanelManager.GetPanel<PaintStripeBackgroundPanel>().Hide();
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
