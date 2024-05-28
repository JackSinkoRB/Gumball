using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SpeedUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI unitLabel;
        [SerializeField] private TextMeshProUGUI speedLabel;
        [SerializeField] private float timeBetweenUpdating = 0.05f;
        
        private float timeOfLastUpdate;
        private float timeSinceLastUpdate => Time.realtimeSinceStartup - timeOfLastUpdate;
        
        private void LateUpdate()
        {
            if (WarehouseManager.Instance.CurrentCar == null)
            {
                speedLabel.text = "0";
                return;
            }

            if (timeSinceLastUpdate < timeBetweenUpdating)
                return;

            UpdateSpeedLabel();
            
            timeOfLastUpdate = Time.realtimeSinceStartup;
        }

        private void UpdateSpeedLabel()
        {
            float speedLocalised = Mathf.Abs(UnitOfSpeedSetting.UseMiles ? SpeedUtils.FromKphToMph(WarehouseManager.Instance.CurrentCar.Speed) : WarehouseManager.Instance.CurrentCar.Speed);
            
            if (speedLocalised < 1)
                speedLocalised = 0;
            float speedToDisplay = Mathf.RoundToInt(speedLocalised);
            speedLabel.text = $"{speedToDisplay}";
            
            unitLabel.text = UnitOfSpeedSetting.UseMiles ? "mph" : "km/h";
        }
        
    }
}
