
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class FPSDisplayUI : MonoBehaviour
    {
        
        private const float timeBetweenUpdates = 0.1f;
        
        [SerializeField] private TextMeshProUGUI label;
        
        private float timeSinceLastUpdate;
        
        private void LateUpdate()
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate < timeBetweenUpdates)
                return;

            timeSinceLastUpdate = 0;
            
            label.text = $"{Mathf.RoundToInt(DynamicResolution.CurrentFPS)} | {Mathf.RoundToInt(DynamicResolution.CurrentScale * 100f)}%";
        }
        
    }
}
