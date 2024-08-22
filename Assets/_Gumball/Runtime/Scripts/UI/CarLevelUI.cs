using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarLevelUI : MonoBehaviour
    {
        
        [SerializeField] private AutosizeTextMeshPro label;
        
        private void OnEnable()
        {
            RefreshLabel();

            BlueprintManager.onLevelChange += OnLevelChange;
        }

        private void OnDisable()
        {
            BlueprintManager.onLevelChange -= OnLevelChange;
        }
        
        private void OnLevelChange(int carIndex, int previousAmount, int newAmount)
        {
            if (WarehouseManager.Instance.CurrentCar != null && carIndex == WarehouseManager.Instance.CurrentCar.CarIndex)
                RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = "";
                return;
            }

            label.text = $"{BlueprintManager.Instance.GetLevelIndex(WarehouseManager.Instance.CurrentCar.CarIndex) + 1}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
