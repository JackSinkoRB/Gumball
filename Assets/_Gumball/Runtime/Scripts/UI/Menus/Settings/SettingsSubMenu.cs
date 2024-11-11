using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SettingsSubMenu : SubMenu
    {

        [SerializeField] private Image iconBackground;

        public override void OnAddToPanelLookup()
        {
            base.OnAddToPanelLookup();
            
            iconBackground.color = iconBackground.color.WithAlphaSetTo(0.1f);
        }

        protected override void OnShow()
        {
            base.OnShow();

            iconBackground.color = iconBackground.color.WithAlphaSetTo(1);
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            iconBackground.color = iconBackground.color.WithAlphaSetTo(0.1f);
        }

    }
}
