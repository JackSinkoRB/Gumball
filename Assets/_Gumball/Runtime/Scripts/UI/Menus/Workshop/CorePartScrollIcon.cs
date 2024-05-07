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
        [SerializeField] private Sprite stockPartIcon;
        
        public void Initialise(CorePart corePart)
        {
            ImageComponent.sprite = corePart == null ? stockPartIcon : corePart.Icon;
            title.text = corePart == null ? "Stock" : corePart.DisplayName;
        }
        
    }
}
