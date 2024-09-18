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
        [Space(5)]        
        [SerializeField] private TextMeshProUGUI typeLabel;
        [SerializeField] private CarLevelUI carLevelUI;
        [SerializeField] private CarBlueprintsUI carBlueprintsUI;
        [SerializeField] private CarRatingUI maxSpeedUI;
        [SerializeField] private CarRatingUI accelerationUI;
        [SerializeField] private CarRatingUI handlingUI;
        [SerializeField] private CarRatingUI nosUI;

        [SerializeField, ReadOnly] private CarOptionUI selectedOption;

        public void Initialise(CarOptionUI selectedOption)
        {
            this.selectedOption = selectedOption;

            makeLabel.text = selectedOption.CarData.MakeDisplayName;
            modelLabel.text = selectedOption.CarData.DisplayName;
            carIcon.sprite = selectedOption.CarData.Icon;

            typeLabel.text = selectedOption.CarData.CarType.ToString();
            carLevelUI.SetCarIndex(selectedOption.CarIndex);
            carBlueprintsUI.SetCarIndex(selectedOption.CarIndex);
            
            PerformanceRatingCalculator RatingCalculator = PerformanceRatingCalculator.GetCalculator(selectedOption.CarData.PerformanceSettings, new CarPerformanceProfile(selectedOption.CarIndex));
            maxSpeedUI.SetRatingCalculator(RatingCalculator);
            accelerationUI.SetRatingCalculator(RatingCalculator);
            handlingUI.SetRatingCalculator(RatingCalculator);
            nosUI.SetRatingCalculator(RatingCalculator);
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
