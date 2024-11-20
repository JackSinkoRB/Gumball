using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CarLevelUI : MonoBehaviour
    {

        [SerializeField] private bool usePlayerCar = true;
        [SerializeField] private AutosizeTextMeshPro label;

        private string carGUID;
        
        public void SetCarGUID(string carGUID)
        {
            this.carGUID = carGUID;
            
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
            label.text = $"{BlueprintManager.Instance.GetLevelIndex(carGUIDToUse) + 1}";
            this.PerformAtEndOfFrame(label.Resize);
        }

    }
}
