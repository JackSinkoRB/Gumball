using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarPerformanceRatingUI : MonoBehaviour
    {
        
        [SerializeField] private AutosizeTextMeshPro ratingLabel;
        
        private void OnEnable()
        {
            RefreshLabel();

            WarehouseManager.Instance.onCurrentCarChanged += OnCarChange;
        }

        private void OnDisable()
        {
            WarehouseManager.Instance.onCurrentCarChanged -= OnCarChange;
        }
        
        private void OnCarChange(AICar newcar)
        {
            RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                ratingLabel.text = "";
                return;
            }

            ratingLabel.text = $"{WarehouseManager.Instance.CurrentCar.CurrentPerformanceRating.TotalRating}";
            this.PerformAtEndOfFrame(ratingLabel.Resize);
        }
        
    }
}
