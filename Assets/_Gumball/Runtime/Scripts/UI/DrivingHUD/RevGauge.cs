using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(Image))]
    public class RevGauge : MonoBehaviour
    {
        
        [Tooltip("The min/max percentages of the image fill that is actually used to display revs.")]
        [SerializeField] private MinMaxFloat usedFillPercent = new(0.1f, 0.74f);
        [SerializeField] private float lerpSpeed = 10f;
        
        private Image image => GetComponent<Image>();

        private void LateUpdate()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
            {
                image.fillAmount = 0;
                return;
            }
            
            float percent = WarehouseManager.Instance.CurrentCar.EngineRpm / WarehouseManager.Instance.CurrentCar.EngineRpmRange.Max;
            float percentAsFillAmount = usedFillPercent.Min + (percent * usedFillPercent.Difference);
            image.fillAmount = Mathf.Lerp(image.fillAmount, percentAsFillAmount, lerpSpeed * Time.deltaTime);
        }

    }
}
