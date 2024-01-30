using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SwitchButton : MonoBehaviour
    {
        
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [SerializeField] private TextMeshProUGUI leftLabel;
        [SerializeField] private TextMeshProUGUI rightLabel;

        [SerializeField] private Color buttonColorSelected;
        [SerializeField] private Color buttonColorUnselected;

        [SerializeField] private float opacitySelected = 1;
        [SerializeField] private float opacityUnselected = 0.33f;

        public virtual void OnClickLeftSwitch()
        {
            SetButtonSelected(true);
        }

        public virtual void OnClickRightSwitch()
        {
            SetButtonSelected(false);
        }
        
        protected void SetButtonSelected(bool isLeft)
        {
            leftButton.image.color = isLeft ? buttonColorSelected : buttonColorUnselected;
            rightButton.image.color = !isLeft ? buttonColorSelected : buttonColorUnselected;
            
            leftLabel.SetAlpha(isLeft ? opacitySelected : opacityUnselected);
            rightLabel.SetAlpha(!isLeft ? opacitySelected : opacityUnselected);
        }
        
    }
}
