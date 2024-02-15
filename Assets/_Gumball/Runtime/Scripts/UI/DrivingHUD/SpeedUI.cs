using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SpeedUI : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI speedLabel;
        [SerializeField] private float timeBetweenUpdating = 0.05f;
        
        private bool carExists => PlayerCarManager.ExistsRuntime && PlayerCarManager.Instance.CurrentCar != null;

        private float timeOfLastUpdate;
        private float timeSinceLastUpdate => Time.realtimeSinceStartup - timeOfLastUpdate;
        
        private void LateUpdate()
        {
            if (!carExists)
            {
                speedLabel.text = "0";
                return;
            }

            if (timeSinceLastUpdate < timeBetweenUpdating)
                return;

            float speedAsKmh = Mathf.Abs(SpeedUtils.ToKmh(PlayerCarManager.Instance.CurrentCar.Speed));
            if (speedAsKmh < 1)
                speedAsKmh = 0;
            
            float speedToDisplay = Mathf.RoundToInt(speedAsKmh);
            
            speedLabel.text = $"{speedToDisplay}";
            
            timeOfLastUpdate = Time.realtimeSinceStartup;
        }
        
    }
}
