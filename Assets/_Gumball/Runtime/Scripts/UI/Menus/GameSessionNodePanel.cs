using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class GameSessionNodePanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI titleLabel;

        [Space(5)]
        [SerializeField] private GameObject objectiveLabelPrefab;
        [SerializeField] private Transform objectivesHolder;

        [Space(5)]
        [SerializeField] private GameSessionNodeReward rewardPrefab;
        [SerializeField] private Transform rewardsHolder;
        [SerializeField] private Sprite standardCurrencyIcon;
        [SerializeField] private Sprite xpIcon;

        private GameSessionNode currentNode;
        
        public void Initialise(GameSessionNode node)
        {
            currentNode = node;
            titleLabel.text = $"{node.GameSession.name}";

            InitialiseObjectives(node.GameSession);
            InitialiseRewards(node.GameSession);
        }

        protected override void OnHide()
        {
            base.OnHide();

            if (MapSceneManager.ExistsRuntime)
                MapSceneManager.Instance.RemoveFocusOnNode();
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
            foreach (Transform child in objectivesHolder)
                child.gameObject.Pool();

            //add main challenge
            TextMeshProUGUI mainChallengeLabel = objectiveLabelPrefab.GetSpareOrCreate<TextMeshProUGUI>(objectivesHolder);
            mainChallengeLabel.text = gameSession.MainChallengeDescription ?? "";
            mainChallengeLabel.transform.SetAsLastSibling();
            
            //add subobjectives
            if (gameSession.SubObjectives != null)
            {
                foreach (Challenge challenge in gameSession.SubObjectives)
                {
                    TextMeshProUGUI challengeLabel = objectiveLabelPrefab.GetSpareOrCreate<TextMeshProUGUI>(objectivesHolder);
                    challengeLabel.text = challenge.Description;
                    challengeLabel.transform.SetAsLastSibling();
                }
            }
        }

        private void InitialiseRewards(GameSession gameSession)
        {
            foreach (Transform child in rewardsHolder)
                child.gameObject.Pool();
            
            //core parts
            foreach (CorePart corePart in gameSession.CorePartRewards)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise(corePart.DisplayName, corePart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //sub parts
            foreach (SubPart subPart in gameSession.SubPartRewards)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise(subPart.DisplayName, subPart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //standard currency
            if (gameSession.StandardCurrencyReward > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise($"${gameSession.StandardCurrencyReward}", standardCurrencyIcon);
                reward.transform.SetAsLastSibling();
            }
            
            //xp
            if (gameSession.XPReward > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder);
                reward.Initialise($"{gameSession.XPReward} EXP", xpIcon);
                reward.transform.SetAsLastSibling();
            }
        }
        
    }
}