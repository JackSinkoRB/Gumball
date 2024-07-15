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

            FuelManager.onFuelChange += OnFuelChange;
        }
        
        private void OnDisable()
        {
            FuelManager.onFuelChange -= OnFuelChange;
        }
        
        private void OnFuelChange(int previousFuel, int newFuel)
        {
            RefreshFuelLabel();
        }
        
        private void RefreshFuelLabel()
        {
            fuelLabel.text = $"{FuelManager.CurrentFuel}";
            this.PerformAtEndOfFrame(fuelLabel.Resize);
        }

    }
}
