using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/GameSession/Clean run")]
    public class CleanRunGameSession : TimedGameSession
    {
        
        public override string GetName()
        {
            return "Clean run";
        }

        protected override GameSessionPanel GetSessionPanel()
        {
            return PanelManager.GetPanel<CleanRunSessionPanel>();
        }
        
        protected override GameSessionEndPanel GetSessionEndPanel()
        {
            return PanelManager.GetPanel<CleanRunSessionEndPanel>();
        }
        
    }
}
