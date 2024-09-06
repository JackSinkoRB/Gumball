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
        
        protected override void OnShow()
        {
            base.OnShow();
            
            SetLevelName();
            PopulateSubObjectives();
        }

        public void PopulateMainObjective(string challengeValue)
        {
            ObjectiveUI objectiveUI = Instantiate(objectiveUIPrefab.gameObject, objectiveUIHolder).GetComponent<ObjectiveUI>();
            objectiveUI.Initialise(GameSessionManager.Instance.CurrentSession.GetChallengeData(), challengeValue);
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
        
    }
}
