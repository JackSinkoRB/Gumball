using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SessionScorePanel : AnimatedPanel
    {

        [SerializeField] private AutosizeTextMeshPro levelNameLabel;
        [SerializeField] private ObjectiveUI objectiveUIPrefab;
        [SerializeField] private Transform objectiveUIHolder;
        [SerializeField] private Transform scoresUIHolder;
        [SerializeField] private AutosizeTextMeshPro totalScoreLabel;
        
        protected override void OnShow()
        {
            base.OnShow();
            
            SetLevelName();
            PopulateSubObjectives();
            PopulateScores();
        }

        public void PopulateMainObjective(string challengeValue)
        {
            ObjectiveUI objectiveUI = Instantiate(objectiveUIPrefab.gameObject, objectiveUIHolder).GetComponent<ObjectiveUI>();
            objectiveUI.Initialise(GameSessionManager.Instance.CurrentSession.GetChallengeData(), challengeValue);
        }

        public void OnClickContinueButton()
        {
            Hide();
            CoroutineHelper.StartCoroutineOnCurrentScene(GiveRewardsThenExitIE());
        }

        private IEnumerator GiveRewardsThenExitIE()
        {
            yield return GameSessionManager.Instance.CurrentSession.Rewards.GiveRewards();
            
            //show the loading panel before unloading as unloading can take some time
            PanelManager.GetPanel<LoadingPanel>().Show();

            GameSessionManager.Instance.CurrentSession.UnloadSession();
            MainSceneManager.LoadMainScene();
        }

        private void SetLevelName()
        {
            levelNameLabel.text = GameSessionManager.Instance.CurrentSession.name;
            levelNameLabel.Resize();
        }

        private void PopulateSubObjectives()
        {
            foreach (Challenge subObjective in GameSessionManager.Instance.CurrentSession.SubObjectives)
            {
                ObjectiveUI objectiveUI = Instantiate(objectiveUIPrefab.gameObject, objectiveUIHolder).GetComponent<ObjectiveUI>();
                objectiveUI.Initialise(subObjective);
            }
        }
        
        private void PopulateScores()
        {
            foreach (SkillCheck skillCheck in SkillCheckManager.Instance.AllSkillChecks)
            {
                int points = Mathf.RoundToInt(skillCheck.PointsSinceSessionStart);
                if (points < 1)
                    continue;
                
                ObjectiveUI objectiveUI = Instantiate(objectiveUIPrefab.gameObject, scoresUIHolder).GetComponent<ObjectiveUI>();
                objectiveUI.Initialise(skillCheck);
                objectiveUI.transform.SetAsFirstSibling();
            }

            totalScoreLabel.text = Mathf.RoundToInt(SkillCheckManager.Instance.CurrentPoints).ToString();
            totalScoreLabel.Resize();
        }
        
    }
}
