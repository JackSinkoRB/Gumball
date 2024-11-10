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

        public override void OnAddToPanelLookup()
        {
            base.OnAddToPanelLookup();

            if (buttonLabel != null)
                buttonLabel.color = buttonLabelColorDeselected;
        }

        protected override void OnShow()
        {
            base.OnShow();

            TweenButtonLabelColor(true);
        }

        protected override void OnHide()
        {
            base.OnHide();

            TweenButtonLabelColor(false);
        }

        private void TweenButtonLabelColor(bool selected)
        {
            if (buttonLabel == null)
                return;
            
            buttonLabelColorTween?.Kill();
            buttonLabelColorTween = buttonLabel.DOColor(selected ? GlobalColourPalette.Instance.GetGlobalColor(buttonLabelColorSelected) : buttonLabelColorDeselected, 
                buttonLabelColorTweenDuration);
        }
        
    }
}
