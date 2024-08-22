using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarBlueprintsUI : MonoBehaviour
    {
        
        [SerializeField] private AutosizeTextMeshPro label;

        private int currentCarIndex => WarehouseManager.Instance.CurrentCar.CarIndex;
        
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
            if (WarehouseManager.Instance.CurrentCar != null && carIndex == currentCarIndex)
                RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = "";
                return;
            }

            int currentBlueprints = BlueprintManager.Instance.GetBlueprints(currentCarIndex);
            
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevel(currentCarIndex);
            int blueprintsForNextLevel = BlueprintManager.Instance.GetBlueprintsRequiredForLevel(nextLevelIndex);
            
            label.text = $"{currentBlueprints}/{blueprintsForNextLevel}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
