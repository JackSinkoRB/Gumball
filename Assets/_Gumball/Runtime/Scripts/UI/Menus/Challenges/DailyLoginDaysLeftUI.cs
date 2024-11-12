using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DailyLoginDaysLeftUI : MonoBehaviour
    {

        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();
        
        private void LateUpdate()
        {
            int daysLeft = DailyLoginManager.Instance.DaysRemainingInCurrentMonth;
            label.text = $"{daysLeft} days left";
        }
        
    }
}
