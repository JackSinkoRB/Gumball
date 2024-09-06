using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Gumball
{
    public class PausePanelDriving : AnimatedPanel
    {

        [SerializeField] private AudioMixer mainAudioMixer;

        protected override void OnShow()
        {
            base.OnShow();
            
            mainAudioMixer.SetFloat("MasterVolume", -80f);
            
            if (PanelManager.GetPanel<DrivingResetButtonPanel>().IsShowing)
                PanelManager.GetPanel<DrivingResetButtonPanel>().Hide(); //hide the reset button
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            mainAudioMixer.SetFloat("MasterVolume", 0f);
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

            MainSceneManager.LoadMainScene();
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
