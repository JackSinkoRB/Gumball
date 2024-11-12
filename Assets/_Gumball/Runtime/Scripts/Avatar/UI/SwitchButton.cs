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
        
        [SerializeField] private Transform leftButtonSelected;
        [SerializeField] private Transform leftButtonDeselected;
        [SerializeField] private Transform rightButtonSelected;
        [SerializeField] private Transform rightButtonDeselected;

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
            leftButtonSelected.gameObject.SetActive(isLeft);
            leftButtonDeselected.gameObject.SetActive(!isLeft);
            rightButtonSelected.gameObject.SetActive(!isLeft);
            rightButtonDeselected.gameObject.SetActive(isLeft);
        }
        
    }
}
