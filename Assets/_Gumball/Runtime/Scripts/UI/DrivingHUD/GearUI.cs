using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GearUI : MonoBehaviour
    {

        private const string reverseLabel = "R";
        private const string neutralLabel = "N";

        [SerializeField] private TextMeshProUGUI gearLabel;

        private bool carExists => WarehouseManager.Instance.CurrentCar != null;

        private void LateUpdate()
        {
            if (!carExists)
            {
                gearLabel.text = neutralLabel;
                return;
            }
            
            gearLabel.text = $"{GetGearAsUserFriendlyString(WarehouseManager.Instance.CurrentCar.CurrentGear)}";
        }

        private string GetGearAsUserFriendlyString(int gear)
        {
            if (gear == 0)
                return reverseLabel;

            return gear.ToString();
        }
        
    }
}
