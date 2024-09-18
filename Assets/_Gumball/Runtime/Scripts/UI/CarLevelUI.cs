using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarLevelUI : MonoBehaviour
    {

        [SerializeField] private bool usePlayerCar = true;
        [SerializeField] private AutosizeTextMeshPro label;

        private int carIndex;
        
        public void SetCarIndex(int carIndex)
        {
            this.carIndex = carIndex;
            
            RefreshLabel();
        }
        
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
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
                return;
            
            int carIndexToUse = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CarIndex : carIndex;
            if (carIndexToUse == carIndex)
                RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = "";
                return;
            }

            int carIndexToUse = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CarIndex : carIndex;
            label.text = $"{BlueprintManager.Instance.GetLevelIndex(carIndexToUse) + 1}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
