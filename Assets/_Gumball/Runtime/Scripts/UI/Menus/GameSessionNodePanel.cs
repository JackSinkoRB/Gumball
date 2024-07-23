using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNodePanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI titleLabel;

        [SerializeField] private GameObject objectiveLabelPrefab;
        [SerializeField] private Transform objectivesHolder;

        private GameSessionNode currentNode;
        
        public void Initialise(GameSessionNode node)
        {
            currentNode = node;
            titleLabel.text = $"{node.GameSession.name}";

            InitialiseObjectives(node);
        }

        private void InitialiseObjectives(GameSessionNode node)
        {
            foreach (Transform child in objectivesHolder)
                child.gameObject.Pool();

            //add main challenge
            TextMeshProUGUI mainChallengeLabel = objectiveLabelPrefab.GetSpareOrCreate<TextMeshProUGUI>(objectivesHolder);
            mainChallengeLabel.text = node.GameSession.MainChallengeDescription ?? "";
            
            //add subobjectives
            if (node.GameSession.SubObjectives != null)
            {
                foreach (Challenge challenge in node.GameSession.SubObjectives)
                {
                    TextMeshProUGUI challengeLabel = objectiveLabelPrefab.GetSpareOrCreate<TextMeshProUGUI>(objectivesHolder);
                    challengeLabel.text = challenge.Description;
                }
            }
        }
        
        protected override void OnHide()
        {
            base.OnHide();

            if (MapSceneManager.ExistsRuntime)
                MapSceneManager.Instance.RemoveFocusOnNode();
        }

        public void OnClickPlayButton()
        {
            if (!FuelManager.HasFuel())
            {
                Hide();
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }
            
            currentNode.GameSession.StartSession();
        }
        
    }
}