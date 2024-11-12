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
        [SerializeField] private Image circle;
        
        [Header("Disabled")]
        [SerializeField] private GlobalColourPalette.ColourCode circleColorDisabled;
        [SerializeField] private float disabledAlpha = 0.5f;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isDisabled;
        
        private AICar racer;

        private CanvasGroup canvasGroup => gameObject.GetOrAddComponent<CanvasGroup>();
        public RectTransform RectTransform => transform as RectTransform;
        public AICar Racer => racer;
        
        public void Initialise(AICar racer)
        {
            this.racer = racer;

            icon.sprite = racer.RacerIcon.CurrentIcon;
            
            RectTransform.anchoredPosition = RectTransform.anchoredPosition.SetX(0);
        }

        public void Disable()
        {
            isDisabled = true;

            circle.color = GlobalColourPalette.Instance.GetGlobalColor(circleColorDisabled);

            canvasGroup.alpha = disabledAlpha;
        }
        
    }
}
