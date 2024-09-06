using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class TimedSessionEndPanel : SessionEndPanel
    {
        
        protected override void OnShow()
        {
            base.OnShow();
            
            ShowPosition(false);
        }
        
    }
}
