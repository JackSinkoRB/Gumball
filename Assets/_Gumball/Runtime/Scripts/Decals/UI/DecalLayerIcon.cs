using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class DecalLayerIcon : ScrollIcon
    {

        [Header("Decal icon")]
        [SerializeField] private TextMeshProUGUI priorityLabel;

        public TextMeshProUGUI PriorityLabel => priorityLabel;
        
    }
}
