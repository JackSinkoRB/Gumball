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
        [SerializeField] private Image fillBottom;
        [SerializeField] private Transform fillBottomLabel;
        [SerializeField] private Image fillBottomEnd;
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
            fillBottom.gameObject.SetActive(comparisonProfile != null);
            fillBottomEnd.gameObject.SetActive(comparisonProfile != null);

            Refresh();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            if (usePlayerCar)
                InitialiseForPlayer();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;
            
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
            AICar.onPerformanceProfileUpdated -= OnCarPerformanceProfileUpdated;
        }
        
        private void LateUpdate()
        {
            UpdateBarEnd();
            UpdateBarEndComparison();
        }

        private void InitialiseForPlayer()
        {
            Initialise(WarehouseManager.Instance.CurrentCar.PerformanceSettings, new CarPerformanceProfile(WarehouseManager.Instance.CurrentCar.CarIndex));

            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
            WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
            
            AICar.onPerformanceProfileUpdated -= OnCarPerformanceProfileUpdated;
            AICar.onPerformanceProfileUpdated += OnCarPerformanceProfileUpdated;
        }
        
        private void OnCarPerformanceProfileUpdated(AICar car)
        {
            if (car == WarehouseManager.Instance.CurrentCar)
                InitialiseForPlayer();
        }
        
        private void OnCarChange(AICar newcar)
        {
            InitialiseForPlayer();
        }

        private void Refresh()
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
            {
                fill.fillAmount = 0;
                valueLabel.text = "";
                return;
            }
            
            PerformanceRatingCalculator calculatorMain = PerformanceRatingCalculator.GetCalculator(settings, profile);
            float ratingPercentMain = (float)calculatorMain.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);

            if (comparisonProfile != null)
            {
                PerformanceRatingCalculator calculatorComparison = PerformanceRatingCalculator.GetCalculator(settings, comparisonProfile.Value);
                float ratingPercentComparison = (float)calculatorComparison.GetRating(ratingComponent) / WarehouseManager.Instance.GetMaxRating(ratingComponent);

                bool increasing = ratingPercentComparison > ratingPercentMain;
                Color comparisonColor = increasing ? GlobalColourPalette.Instance.GetGlobalColor(comparisonIncreaseColourCode) : GlobalColourPalette.Instance.GetGlobalColor(comparisonDecreaseColourCode);
                fillBottom.color = comparisonColor;
                fillBottomEnd.color = comparisonColor;
                
                fill.fillAmount = increasing ? ratingPercentMain : ratingPercentComparison;
                fillBottom.fillAmount = increasing ? ratingPercentComparison : ratingPercentMain;
                
                valueLabel.text = $"{calculatorComparison.GetRating(ratingComponent)}";
            }
            else
            {
                fill.fillAmount = ratingPercentMain;
                valueLabel.text = $"{calculatorMain.GetRating(ratingComponent)}";
            }
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
            if (fillBottomEnd == null || fillBottom == null)
                return;
            
            fillBottomEnd.rectTransform.anchoredPosition = fillBottomEnd.rectTransform.anchoredPosition.SetX(fillBottom.fillAmount * (fillBottom.rectTransform.rect.width * fillBottom.rectTransform.localScale.x));
            
            //return children to fill end transform
            foreach (RectTransform child in fillBottomEnd.transform)
                child.position = fillBottomLabel.position;
        }
        
    }
}
