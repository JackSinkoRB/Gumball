using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class WorkshopSubMenu : SubMenu
    {

        [SerializeField] private Image buttonLabel;
        [SerializeField] private GlobalColourPalette.ColourCode buttonLabelColorSelected;
        [SerializeField] private Color buttonLabelColorDeselected;
        [SerializeField] private float buttonLabelColorTweenDuration = 0.2f;

        private Tween buttonLabelColorTween;
        
        protected override void OnShow()
        {
            base.OnShow();

            if (buttonLabel != null)
            {
                buttonLabelColorTween?.Kill();        
                buttonLabelColorTween = buttonLabel.DOColor(GlobalColourPalette.Instance.GetGlobalColor(buttonLabelColorSelected), buttonLabelColorTweenDuration);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (buttonLabel != null)
            {
                buttonLabelColorTween?.Kill();
                buttonLabelColorTween = buttonLabel.DOColor(buttonLabelColorDeselected, buttonLabelColorTweenDuration);
            }
        }
        
    }
}
