using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Gumball
{
    public class PausePanelDriving : AnimatedPanel
    {

        protected override void OnShow()
        {
            base.OnShow();
            
            if (PanelManager.GetPanel<DrivingResetButtonPanel>().IsShowing)
                PanelManager.GetPanel<DrivingResetButtonPanel>().Hide(); //hide the reset button
        }

        public void OnClickSettingsButton()
        {
            Hide(true);
            PanelManager.GetPanel<SettingsPanel>().Show();
        }

        public void OnClickQuitButton()
        {
            if (GameSessionManager.ExistsRuntime && GameSessionManager.Instance.CurrentSession != null)
            {
                GameSessionManager.Instance.CurrentSession.EndSession(GameSession.ProgressStatus.ATTEMPTED);
                GameSessionManager.Instance.CurrentSession.UnloadSession();
            }

            MapSceneManager.LoadMapScene();
        }

        public override void OnAddToStack()
        {
            base.OnAddToStack();
            
            Time.timeScale = 0;
        }

        public override void OnRemoveFromStack()
        {
            base.OnRemoveFromStack();
            
            Time.timeScale = 1;
        }
        
    }
}
