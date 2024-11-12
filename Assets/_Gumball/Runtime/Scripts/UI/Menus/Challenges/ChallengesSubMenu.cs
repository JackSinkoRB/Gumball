using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class ChallengesSubMenu : SubMenu
    {

        [SerializeField] private Button categoryButton;

        private ChallengesPanel challengesPanel => PanelManager.GetPanel<ChallengesPanel>();
        
        protected override void OnShow()
        {
            base.OnShow();

            this.PerformAtEndOfFrame(() =>
            {
                challengesPanel.Header.Select(categoryButton);
            });
        }
        
    }
}