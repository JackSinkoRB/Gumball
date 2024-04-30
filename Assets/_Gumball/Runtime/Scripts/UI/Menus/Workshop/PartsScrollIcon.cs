using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class PartsScrollIcon : ScrollIcon
    {
        
        [Header("Parts scroll icon")]
        [SerializeField] private TextMeshProUGUI displayNameLabel;

        public TextMeshProUGUI DisplayNameLabel => displayNameLabel;
        
    }
}
