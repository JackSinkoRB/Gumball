using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class RewardScrollIcon : ScrollIcon
    {

        [SerializeField] private TextMeshProUGUI label;

        public void Initialise(string displayName, Sprite icon)
        {
            ImageComponent.sprite = icon;
            label.text = displayName;
        }

    }
}
