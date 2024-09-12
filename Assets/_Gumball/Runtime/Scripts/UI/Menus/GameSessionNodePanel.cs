using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNodePanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI modeLabel;

        [Space(5)]
        [SerializeField] private ObjectiveUI objectiveUIPrefab;
        [SerializeField] private Transform objectiveUIHolder;

        [Space(5)]
        [SerializeField] private GameSessionNodeReward rewardPrefab;
        [SerializeField] private Transform rewardsHolder;
        [SerializeField] private Sprite standardCurrencyIcon;
        [SerializeField] private Sprite xpIcon;

        private GameSessionNode currentNode;
        
        public void Initialise(GameSessionNode node)
        {
            currentNode = node;
            titleLabel.text = $"{node.GameSession.DisplayName}";
            modeLabel.text = $"{node.GameSession.GetModeDisplayName()}";

            InitialiseObjectives(node.GameSession);
            InitialiseRewards(node.GameSession);
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (MapSceneManager.ExistsRuntime)
                MapSceneManager.Instance.RemoveFocusOnNode();
            
            if (PanelManager.PanelExists<GameSessionMapPanel>())
                PanelManager.GetPanel<GameSessionMapPanel>().Show();
        }

        public void OnClickPlayButton()
        {
            if (!FuelManager.Instance.HasFuel())
            {
                Hide();
                PanelManager.GetPanel<InsufficientFuelPanel>().Show();
                return;
            }
            
            currentNode.GameSession.StartSession();
        }
        
        private void InitialiseObjectives(GameSession gameSession)
        {
            foreach (Transform child in objectiveUIHolder)
                child.gameObject.Pool();

            //add main challenge
            ObjectiveUI mainObjectiveUI = objectiveUIPrefab.gameObject.GetSpareOrCreate<ObjectiveUI>(objectiveUIHolder);
            mainObjectiveUI.Initialise(gameSession.GetChallengeData(), gameSession.GetMainObjectiveGoalValue());
            mainObjectiveUI.transform.SetAsLastSibling();
            
            //add subobjectives
            if (gameSession.SubObjectives != null)
            {
                foreach (Challenge subObjective in gameSession.SubObjectives)
                {
                    ObjectiveUI objectiveUI = objectiveUIPrefab.gameObject.GetSpareOrCreate<ObjectiveUI>(objectiveUIHolder);
                    objectiveUI.Initialise(subObjective);
                    objectiveUI.transform.SetAsLastSibling();
                }
            }
        }

        private void InitialiseRewards(GameSession gameSession)
        {
            foreach (Transform child in rewardsHolder)
                child.gameObject.Pool();
            
            //core parts
            foreach (CorePart corePart in gameSession.Rewards.CoreParts)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise(corePart.DisplayName, corePart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //sub parts
            foreach (SubPart subPart in gameSession.Rewards.SubParts)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise(subPart.DisplayName, subPart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //standard currency
            if (gameSession.Rewards.StandardCurrency > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise($"${gameSession.Rewards.StandardCurrency}", standardCurrencyIcon);
                reward.transform.SetAsLastSibling();
            }
            
            //xp
            if (gameSession.Rewards.XP > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise($"{gameSession.Rewards.XP} EXP", xpIcon);
                reward.transform.SetAsLastSibling();
            }
        }
        
    }
}