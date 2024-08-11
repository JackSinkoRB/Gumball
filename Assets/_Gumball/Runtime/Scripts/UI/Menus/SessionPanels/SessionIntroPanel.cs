using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SessionIntroPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI countdownLabel;

        public void UpdateCountdownLabel(string text)
        {
            countdownLabel.text = text;
        }

    }
}
