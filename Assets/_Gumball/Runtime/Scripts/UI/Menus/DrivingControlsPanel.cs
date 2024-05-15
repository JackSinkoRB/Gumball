using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlsPanel : AnimatedPanel
    {

        [SerializeField] private DrivingControlLayoutManager layoutManager;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            layoutManager.ShowCurrentLayout();
        }
        
    }
}
