using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GenericMessagePanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI messageLabel;

        public void Initialise(string message)
        {
            messageLabel.text = message;
        }

    }
}
