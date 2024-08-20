using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class CarRatingUI : MonoBehaviour
    {

        [SerializeField] private PerformanceRatingCalculator.Component ratingComponent; 
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI valueLabel;

        private void OnEnable()
        {
            Refresh();

            WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
        }
        
        private void OnCarChange(AICar newcar)
        {
            Refresh();
        }
        
        private void Refresh()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                fillBar.fillAmount = 0;
                valueLabel.text = "";
                return;
            }
            
            PerformanceRatingCalculator calculator = WarehouseManager.Instance.CurrentCar.CurrentPerformanceRating;
            float ratingPercentComparedToMax = (float)calculator.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);
            fillBar.fillAmount = ratingPercentComparedToMax;

            valueLabel.text = $"{calculator.GetRating(ratingComponent)}";
        }

    }
}
