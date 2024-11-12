using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlsPanel : AnimatedPanel
    {

        [SerializeField] private DrivingControlLayoutManager layoutManager;

        public DrivingControlLayoutManager LayoutManager => layoutManager;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            layoutManager.ShowCurrentLayout();
        }

        public void OnClickCameraSwitchButton()
        {
            ChunkMapSceneManager.Instance.DrivingCameraController.SetNextDrivingState();
        }

        public void OnClickPauseButton()
        {
            PanelManager.GetPanel<VignetteBackgroundPanel>().Show();
            PanelManager.GetPanel<PausePanelDriving>().Show();
        }
        
    }
}
