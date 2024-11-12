using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class KnockoutPositionIcon : MonoBehaviour
    {
        
        [SerializeField] private Image icon;
        [SerializeField] private Sprite passedIcon;
        
        public void SetPassed()
        {
            icon.sprite = passedIcon;
        }
        
    }
}
