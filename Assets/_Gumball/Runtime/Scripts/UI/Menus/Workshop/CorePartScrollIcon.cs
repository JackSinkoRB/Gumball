using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class CorePartScrollIcon : ScrollIcon
    {

        [SerializeField] private TextMeshProUGUI title;

        public void Initialise(CorePart corePart)
        {
            ImageComponent.sprite = corePart.Icon;
            title.text = corePart.DisplayName;
        }
        
    }
}
