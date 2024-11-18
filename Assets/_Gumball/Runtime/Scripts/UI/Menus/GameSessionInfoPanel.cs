using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public abstract class GameSessionInfoPanel : AnimatedPanel
    {
        
        [SerializeField] private TextMeshProUGUI sessionNameLabel;
        [SerializeField] private Image modeIconImage;

        [Space(5)]
        [SerializeField] private ObjectiveUI objectiveUIPrefab;
        [SerializeField] private Transform objectiveUIHolder;

        [Space(5)]
        [SerializeField] private GameSessionNodeReward rewardPrefab;
        [SerializeField] private Transform rewardsHolder;
        [SerializeField] private Sprite standardCurrencyIcon;
        [SerializeField] private Sprite premiumCurrencyIcon;
        [SerializeField] private Sprite xpIcon;

        protected GameSession session;
        
        public virtual void Initialise(GameSession session)
        {
            this.session = session;
            
            sessionNameLabel.text = session.DisplayName;
            modeIconImage.sprite = session.GetModeIcon();

            InitialiseObjectives(session);
            InitialiseRewards(session);
        }

        private void InitialiseObjectives(GameSession gameSession)
        {
            foreach (Transform child in objectiveUIHolder)
                child.gameObject.Pool();

            //add main challenge
            ObjectiveUI mainObjectiveUI = objectiveUIPrefab.gameObject.GetSpareOrCreate<ObjectiveUI>(objectiveUIHolder, poolOnDisable: false);
            mainObjectiveUI.Initialise(gameSession.GetChallengeData(), gameSession.GetMainObjectiveGoalValue());
            mainObjectiveUI.transform.SetAsLastSibling();
            
            //add subobjectives
            if (gameSession.SubObjectives != null)
            {
                foreach (Challenge subObjective in gameSession.SubObjectives)
                {
                    ObjectiveUI objectiveUI = objectiveUIPrefab.gameObject.GetSpareOrCreate<ObjectiveUI>(objectiveUIHolder, poolOnDisable: false);
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
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise(corePart.DisplayName, corePart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //sub parts
            foreach (SubPart subPart in gameSession.Rewards.SubParts)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise(subPart.DisplayName, subPart.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //blueprints
            foreach (BlueprintReward blueprintReward in gameSession.Rewards.Blueprints)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise(blueprintReward.Blueprints.ToString(), WarehouseManager.Instance.GetCarDataFromGUID(blueprintReward.CarGUID).Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //unlockables
            foreach (Unlockable unlockableReward in gameSession.Rewards.Unlockables)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise("1", unlockableReward.Icon);
                reward.transform.SetAsLastSibling();
            }
            
            //standard currency
            if (gameSession.Rewards.StandardCurrency > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise($"${gameSession.Rewards.StandardCurrency}", standardCurrencyIcon);
                reward.transform.SetAsLastSibling();
            }
            
            //premium currency
            if (gameSession.Rewards.PremiumCurrency > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise($"${gameSession.Rewards.PremiumCurrency}", premiumCurrencyIcon);
                reward.transform.SetAsLastSibling();
            }
            
            //xp
            if (gameSession.Rewards.XP > 0)
            {
                GameSessionNodeReward reward = rewardPrefab.gameObject.GetSpareOrCreate<GameSessionNodeReward>(rewardsHolder, poolOnDisable: false);
                reward.Initialise($"{gameSession.Rewards.XP}xp", xpIcon);
                reward.transform.SetAsLastSibling();
            }
        }
        
    }
}
