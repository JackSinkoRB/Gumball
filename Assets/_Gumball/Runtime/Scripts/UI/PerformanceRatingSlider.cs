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
        [Tooltip("Does the slider update whenever the player's car updates with the cars stats? If disabled, it will need to be initialised with the Initialise() method.")]
        [SerializeField] private bool usePlayerCar = true;
        [Space(5)]
        [SerializeField] private Image fill;
        [SerializeField] private Transform fillLabel;
        [SerializeField] private Image fillEnd;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [Space(5)]
        [SerializeField] private Image fillComparison;
        [SerializeField] private Transform fillLabelComparison;
        [SerializeField] private Image fillEndComparison;
        [SerializeField] private GlobalColourPalette.ColourCode comparisonIncreaseColourCode;
        [SerializeField] private GlobalColourPalette.ColourCode comparisonDecreaseColourCode;
        
        private CarPerformanceSettings settings;
        private CarPerformanceProfile profile;
        private CarPerformanceProfile? comparisonProfile;
        
        public void Initialise(CarPerformanceSettings settings, CarPerformanceProfile profile, CarPerformanceProfile? comparisonProfile = null)
        {
            this.settings = settings;
            this.profile = profile;
            this.comparisonProfile = comparisonProfile;

            //not comparing
            fillComparison.gameObject.SetActive(comparisonProfile != null);
            fillEndComparison.gameObject.SetActive(comparisonProfile != null);

            Refresh();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            if (usePlayerCar)
            {
                Initialise(WarehouseManager.Instance.CurrentCar.PerformanceSettings, new CarPerformanceProfile(WarehouseManager.Instance.CurrentCar.CarIndex));
                WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;
            
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
        }
        
        private void OnCarChange(AICar newcar)
        {
            Refresh();
        }

        private void LateUpdate()
        {
            UpdateBarEnd();
            UpdateBarEndComparison();
        }

        private void Refresh()
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
            {
                fill.fillAmount = 0;
                valueLabel.text = "";
                return;
            }
            
            PerformanceRatingCalculator calculator = PerformanceRatingCalculator.GetCalculator(settings, profile);
            float ratingPercentComparedToMax = (float)calculator.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);
            fill.fillAmount = ratingPercentComparedToMax;

            if (comparisonProfile != null)
            {
                PerformanceRatingCalculator calculatorComparison = PerformanceRatingCalculator.GetCalculator(settings, comparisonProfile.Value);
                float ratingPercentComparedToMaxComparison = (float)calculatorComparison.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);
                fillComparison.fillAmount = ratingPercentComparedToMaxComparison;
                
                Color comparisonColor = fillComparison.fillAmount > fill.fillAmount ? GlobalColourPalette.Instance.GetGlobalColor(comparisonIncreaseColourCode) : GlobalColourPalette.Instance.GetGlobalColor(comparisonDecreaseColourCode);
                fillComparison.color = comparisonColor;
                fillEndComparison.color = comparisonColor;
                
                //TODO: if the  
            }


            
            valueLabel.text = $"{calculator.GetRating(ratingComponent)}";
        }

        private void UpdateBarEnd()
        {
            if (fillEnd == null || fill == null)
                return;
            
            fillEnd.rectTransform.anchoredPosition = fillEnd.rectTransform.anchoredPosition.SetX(fill.fillAmount * (fill.rectTransform.rect.width * fill.rectTransform.localScale.x));
            
            //return children to fill end transform
            foreach (RectTransform child in fillEnd.transform)
                child.position = fillLabel.position;
        }
        
        private void UpdateBarEndComparison()
        {
            if (fillEndComparison == null || fillComparison == null)
                return;
            
            fillEndComparison.rectTransform.anchoredPosition = fillEndComparison.rectTransform.anchoredPosition.SetX(fillComparison.fillAmount * (fillComparison.rectTransform.rect.width * fillComparison.rectTransform.localScale.x));
            
            //return children to fill end transform
            foreach (RectTransform child in fillEndComparison.transform)
                child.position = fillLabelComparison.position;
        }
        
    }
}
