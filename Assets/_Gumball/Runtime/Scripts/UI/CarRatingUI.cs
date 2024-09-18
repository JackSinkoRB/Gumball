using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [ExecuteAlways]
    public class CarRatingUI : MonoBehaviour
    {

        [SerializeField] private PerformanceRatingCalculator.Component ratingComponent;
        [SerializeField] private bool usePlayerCar = true;
        [SerializeField] private Image fill;
        [SerializeField] private RectTransform fillEnd;
        [SerializeField] private TextMeshProUGUI valueLabel;

        private PerformanceRatingCalculator ratingCalculator;
        
        public void SetRatingCalculator(PerformanceRatingCalculator ratingCalculator)
        {
            this.ratingCalculator = ratingCalculator;
            
            Refresh();
        }
        
        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;
            
            Refresh();

            if (usePlayerCar)
                WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;
            
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
        }
        
        private void LateUpdate()
        {
            UpdateBarEnd();
        }

        private void OnCarChange(AICar newcar)
        {
            Refresh();
        }
        
        private void Refresh()
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
            {
                fill.fillAmount = 0;
                valueLabel.text = "";
                return;
            }
            
            PerformanceRatingCalculator calculator = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CurrentPerformanceRating : ratingCalculator;
            float ratingPercentComparedToMax = (float)calculator.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);
            fill.fillAmount = ratingPercentComparedToMax;

            valueLabel.text = $"{calculator.GetRating(ratingComponent)}";
        }
        
        private void UpdateBarEnd()
        {
            if (fillEnd == null || fill == null)
                return;
            
            fillEnd.anchoredPosition = fillEnd.anchoredPosition.SetX(fill.fillAmount * (fill.rectTransform.rect.width * fill.rectTransform.localScale.x));
        }

    }
}
