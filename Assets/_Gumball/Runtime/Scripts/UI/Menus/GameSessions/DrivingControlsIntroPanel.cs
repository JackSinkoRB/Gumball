using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlsIntroPanel : AnimatedPanel
    {

        [SerializeField] private GameObject screenSpace1Layout;
        [SerializeField] private GameObject screenSpace2Layout;
        [SerializeField] private GameObject tiltLayout;
        [SerializeField] private GameObject steeringWheelLayout;

        private DrivingControlLayoutManager layoutManager => PanelManager.GetPanel<DrivingControlsPanel>().LayoutManager;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            screenSpace1Layout.gameObject.SetActive(layoutManager.CurrentLayout.Type == DrivingControlLayout.LayoutType.ScreenSpace1);
            screenSpace2Layout.gameObject.SetActive(layoutManager.CurrentLayout.Type == DrivingControlLayout.LayoutType.ScreenSpace2);
            tiltLayout.gameObject.SetActive(layoutManager.CurrentLayout.Type == DrivingControlLayout.LayoutType.Tilt);
            steeringWheelLayout.gameObject.SetActive(layoutManager.CurrentLayout.Type == DrivingControlLayout.LayoutType.SteeringWheel);
        }
        
    }
}
