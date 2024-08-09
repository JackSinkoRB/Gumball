using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarNameUI : MonoBehaviour
    {
        
        [SerializeField] private AutosizeTextMeshPro nameLabel;
        
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
                nameLabel.text = "";
                return;
            }

            nameLabel.text = $"{WarehouseManager.Instance.CurrentCar.DisplayName}";
            this.PerformAtEndOfFrame(nameLabel.Resize);
        }

    }
}
