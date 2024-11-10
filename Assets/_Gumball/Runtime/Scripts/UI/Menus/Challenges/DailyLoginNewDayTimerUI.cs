using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DailyLoginNewDayTimerUI : MonoBehaviour
    {

        private TextMeshProUGUI label => GetComponent<TextMeshProUGUI>();
        
        private void LateUpdate()
        {
            long timeLeftInDaySeconds = DailyLoginManager.Instance.SecondsLeftInCurrentDay;
            label.text = TimeSpan.FromSeconds(timeLeftInDaySeconds).ToPrettyString();
        }
        
    }
}
