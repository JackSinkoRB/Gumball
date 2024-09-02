using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [ExecuteAlways]
    public class PerformanceRatingSlider : MonoBehaviour
    {

        [SerializeField] private PerformanceRatingCalculator.Component ratingComponent;
        [SerializeField] private Image fill;
        [SerializeField] private RectTransform fillEnd;
        [SerializeField] private TextMeshProUGUI valueLabel;

        public void UpdateProfile(CarPerformanceSettings settings, CarPerformanceProfile profile)
        {
            PerformanceRatingCalculator calculator = PerformanceRatingCalculator.GetCalculator(settings, profile);
            float ratingPercentComparedToMax = (float)calculator.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);
            fill.fillAmount = ratingPercentComparedToMax;

            valueLabel.text = $"{calculator.GetRating(ratingComponent)}";
        }

        private void LateUpdate()
        {
            UpdateBarEnd();
        }

        private void UpdateBarEnd()
        {
            if (fillEnd == null || fill == null)
                return;
            
            fillEnd.anchoredPosition = fillEnd.anchoredPosition.SetX(fill.fillAmount * (fill.rectTransform.rect.width * fill.rectTransform.localScale.x));
        }
        
    }
}
