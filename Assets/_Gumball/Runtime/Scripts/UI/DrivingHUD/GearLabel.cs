using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GearLabel : MonoBehaviour
    {

        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();

        private void LateUpdate()
        {
            if (!WarehouseManager.HasLoaded || WarehouseManager.Instance.CurrentCar == null)
            {
                label.text = GetFinalString("N");
                return;
            }

            label.text = GetFinalString(GetGearAsFriendlyString(WarehouseManager.Instance.CurrentCar.CurrentGear));
        }

        private string GetFinalString(string gearString)
        {
            return $"<size=50>{gearString}</size>\nGear";
        }
        
        private string GetGearAsFriendlyString(int gear)
        {
            return gear switch
            {
                0 => "R",
                1 => "1st",
                2 => "2nd",
                3 => "3rd",
                _ => $"{gear}th"
            };
        }
        
    }
}
