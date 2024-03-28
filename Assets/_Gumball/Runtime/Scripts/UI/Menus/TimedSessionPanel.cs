using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI timerLabel;
        [SerializeField] private TextMeshProUGUI distanceLabel;

        public TextMeshProUGUI TimerLabel => timerLabel;
        public TextMeshProUGUI DistanceLabel => distanceLabel;

    }
}
