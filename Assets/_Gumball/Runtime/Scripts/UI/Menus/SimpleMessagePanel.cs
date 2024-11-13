using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class SimpleMessagePanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI label;

        public void Initialise(string text)
        {
            label.text = text;
        }

    }
}
