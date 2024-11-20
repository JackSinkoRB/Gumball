using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarBlueprintsUI : MonoBehaviour
    {
        
        [SerializeField] private bool usePlayerCar = true;
        [SerializeField] private AutosizeTextMeshPro label;
        
        private string carGUID;

        public void SetCarIndex(string carGUID)
        {
            this.carGUID = carGUID;
            
            RefreshLabel();
        }
        
        private void OnEnable()
        {
            RefreshLabel();

            BlueprintManager.onBlueprintsChange += OnBlueprintsChange;
            BlueprintManager.onLevelChange += OnLevelChange;
        }

        private void OnDisable()
        {
            BlueprintManager.onBlueprintsChange -= OnBlueprintsChange;
            BlueprintManager.onLevelChange -= OnLevelChange;
        }
        
        private void OnBlueprintsChange(string carGUID, int previousAmount, int newAmount)
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
                return;
            
            string carGUIDToUse = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CarGUID : carGUID;
            if (carGUIDToUse.Equals(carGUID))
                RefreshLabel();
        }
        
        private void OnLevelChange(string carGUID, int previousAmount, int newAmount)
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
                return;
            
            string carGUIDToUse = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CarGUID : carGUID;
            if (carGUIDToUse.Equals(carGUID))
                RefreshLabel();
        }
        
        private void RefreshLabel()
        {
            if (usePlayerCar && WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = "";
                return;
            }

            string carGUIDToUse = usePlayerCar ? WarehouseManager.Instance.CurrentCar.CarGUID : carGUID;
            int currentBlueprints = BlueprintManager.Instance.GetBlueprints(carGUIDToUse);
            
            int nextLevelIndex = BlueprintManager.Instance.GetNextLevelIndex(carGUIDToUse);
            if (nextLevelIndex < 0)
            {
                //is max level
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            int blueprintsForNextLevel = BlueprintManager.Instance.Levels[nextLevelIndex].BlueprintsRequired;
            
            label.text = $"<color=white>{currentBlueprints}/</color>{blueprintsForNextLevel}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
