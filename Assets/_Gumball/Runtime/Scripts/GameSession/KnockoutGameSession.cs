using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Knockout")]
    public class KnockoutGameSession : GameSession
    {

        public override string GetName()
        {
            return "Knockout";
        }
        
        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionPanel>();
        }

        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<KnockoutSessionEndPanel>();
        }

    }
}
