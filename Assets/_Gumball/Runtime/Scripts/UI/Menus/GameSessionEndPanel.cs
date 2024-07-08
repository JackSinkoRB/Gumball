using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GameSessionEndPanel : AnimatedPanel
    {
        
        public void OnClickExitButton()
        {
            StartCoroutine(GiveRewardsThenExitIE());
        }

        private IEnumerator GiveRewardsThenExitIE()
        {
            yield return GameSessionManager.Instance.CurrentSession.GiveRewards();
            
            GameSessionManager.Instance.CurrentSession.UnloadSession();
            MainSceneManager.LoadMainScene();
        }
        
    }
}
