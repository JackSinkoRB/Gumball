using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class SessionEndPanel : AnimatedPanel
    {

        private const float secondsToShowPanel = 2.5f;
        [SerializeField] private AutosizeTextMeshPro levelNameLabel;
        [Space(5)]
        [SerializeField] private TextMeshProUGUI positionLabel;
        [Space(5)]
        [SerializeField] private AutosizeTextMeshPro victoryDefeatLabel;
        [SerializeField] private string victoryText = "Victory!";
        [SerializeField] private string defeatText = "Try again";
        [Space(5)]
        [SerializeField] private GlobalColourAssigner[] colourAssignersVictoryDefeat;
        [SerializeField] private GlobalColourPalette.ColourCode victoryColourCode;
        [SerializeField] private GlobalColourPalette.ColourCode defeatColourCode;
        [Space(5)]
        [SerializeField] private GameObject confettiParticles;

        private Coroutine showScorePanelCoroutine;

        protected override void OnShow()
        {
            base.OnShow();
            
            SetLevelName();
            SetVictory(GameSessionManager.Instance.CurrentSession.LastProgress == GameSession.ProgressStatus.COMPLETE);

            showScorePanelCoroutine = this.PerformAfterDelay(secondsToShowPanel, () =>
            {
                if (!IsShowing)
                    return;
                
                Hide();
                PanelManager.GetPanel<SessionScorePanel>().Show();
                OnShowScorePanel();
            });
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (showScorePanelCoroutine != null)
                StopCoroutine(showScorePanelCoroutine);
        }

        protected abstract void OnShowScorePanel();

        private void SetLevelName()
        {
            levelNameLabel.text = GameSessionManager.Instance.CurrentSession.name;
            levelNameLabel.Resize();
        }
        
        protected void SetVictory(bool isVictory)
        {
            confettiParticles.gameObject.SetActive(isVictory);

            foreach (GlobalColourAssigner colourAssigner in colourAssignersVictoryDefeat)
                colourAssigner.SetColour(isVictory ? victoryColourCode : defeatColourCode);

            victoryDefeatLabel.text = isVictory ? victoryText : defeatText;
            victoryDefeatLabel.Resize();
        }

        protected void SetPosition(int position)
        {
            positionLabel.text = position.ToOrdinalString();
        }

    }
}
