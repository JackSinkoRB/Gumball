using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarBlueprintsUI : MonoBehaviour
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

            BlueprintManager.onBlueprintsChange += OnBlueprintsChange;
        }

        private void OnDisable()
        {
            BlueprintManager.onBlueprintsChange -= OnBlueprintsChange;
        }
        
        private void OnBlueprintsChange(int carIndex, int previousAmount, int newAmount)
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
            int currentBlueprints = BlueprintManager.Instance.GetBlueprints(carIndexToUse);
            
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevel(carIndexToUse);
            int blueprintsForNextLevel = BlueprintManager.Instance.GetBlueprintsRequiredForLevel(nextLevelIndex);
            
            label.text = $"{currentBlueprints}/{blueprintsForNextLevel}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
