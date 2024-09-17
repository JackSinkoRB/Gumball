using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SessionProgressBarRacerIcon : MonoBehaviour
    {

        [SerializeField] private Image icon;

        private AICar racer;

        public RectTransform RectTransform => transform as RectTransform;
        public AICar Racer => racer;
        
        public void Initialise(AICar racer)
        {
            this.racer = racer;

            icon.sprite = racer.RacerIcon.CurrentIcon;
            
            RectTransform.anchoredPosition = RectTransform.anchoredPosition.SetX(0);
        }

    }
}
