using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class GameSessionEndPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI totalPointsLabel;

        protected override void OnShow()
        {
            base.OnShow();

            UpdateTotalPointsLabel();
        }

        public void OnClickExitButton()
        {
            Hide();
            CoroutineHelper.Instance.StartCoroutine(GiveRewardsThenExitIE());
        }

        private void UpdateTotalPointsLabel()
        {
            totalPointsLabel.text = $"{Mathf.RoundToInt(SkillCheckManager.Instance.CurrentPoints)}";
        }
        
        private IEnumerator GiveRewardsThenExitIE()
        {
            yield return GameSessionManager.Instance.CurrentSession.Rewards.GiveRewards();
            
            GameSessionManager.Instance.CurrentSession.UnloadSession();
            MainSceneManager.LoadMainScene();
        }
        
    }
}
