using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class FuelUI : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro fuelLabel;
        
        private void OnEnable()
        {
            RefreshFuelLabel();

            FuelManager.Instance.onFuelChange += OnFuelChange;
        }
        
        private void OnDisable()
        {
            FuelManager.Instance.onFuelChange -= OnFuelChange;
        }
        
        private void OnFuelChange(int previousFuel, int newFuel)
        {
            RefreshFuelLabel();
        }
        
        private void RefreshFuelLabel()
        {
            fuelLabel.text = $"{FuelManager.Instance.CurrentFuel}";
            this.PerformAtEndOfFrame(fuelLabel.Resize);
        }

    }
}
