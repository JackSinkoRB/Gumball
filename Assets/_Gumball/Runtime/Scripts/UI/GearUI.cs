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

        private bool carExists => PlayerCarManager.ExistsRuntime && PlayerCarManager.Instance.CurrentCar != null;

        private void LateUpdate()
        {
            if (!carExists)
            {
                gearLabel.text = neutralLabel;
                return;
            }
            
            gearLabel.text = $"{GetGearAsUserFriendlyString(PlayerCarManager.Instance.CurrentCar.drivetrain.Gear)}";
        }

        private string GetGearAsUserFriendlyString(int gear)
        {
            if (gear == 0)
                return reverseLabel;

            if (gear == 1)
                return neutralLabel;

            return (gear - 1).ToString();
        }
        
    }
}
