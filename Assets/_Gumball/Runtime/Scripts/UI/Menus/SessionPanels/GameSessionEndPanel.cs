using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class GameSessionEndPanel : AnimatedPanel
    {

        public void OnClickExitButton()
        {
            Hide();
            CoroutineHelper.Instance.StartCoroutine(GiveRewardsThenExitIE());
        }

        private IEnumerator GiveRewardsThenExitIE()
        {
            yield return GameSessionManager.Instance.CurrentSession.Rewards.GiveRewards();
            
            GameSessionManager.Instance.CurrentSession.UnloadSession();
            MainSceneManager.LoadMainScene();
        }
        
    }
}
