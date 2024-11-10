using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ChallengeUI : MonoBehaviour
    {

        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI progressLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image icon;
        [SerializeField] private Transform rewardHolder;
        [SerializeField] private Transform rewardPrefab;
        [SerializeField] private Transform notificationIcon;
        [Space(5)]
        [SerializeField] private Sprite standardCurrencyIcon;
        [SerializeField] private Sprite premiumCurrencyIcon;
        [SerializeField] private Sprite xpIcon;
        [SerializeField] private Sprite fuelRefillIcon;

        private Challenge challenge;
        private Challenges challengeManager;
        private bool isUnclaimedChallenge;
        
        public void Initialise(Challenge challenge, Challenges challengeManager, bool isUnclaimedChallenge)
        {
            this.challenge = challenge;
            this.challengeManager = challengeManager;
            this.isUnclaimedChallenge = isUnclaimedChallenge;

            UpdateDescriptionLabel();
            UpdateDescriptionIcon();
            UpdateProgressLabel();
            UpdateClaimButton();
            UpdateRewards();
        }

        private void UpdateRewards()
        {
            foreach (Transform child in rewardHolder)
                child.gameObject.Pool();
            
            if (challenge.Rewards.StandardCurrency > 0)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(standardCurrencyIcon, challenge.Rewards.StandardCurrency.ToString());
            }
            
            if (challenge.Rewards.PremiumCurrency > 0)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(premiumCurrencyIcon, challenge.Rewards.PremiumCurrency.ToString());
            }
            
            if (challenge.Rewards.XP > 0)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(xpIcon, challenge.Rewards.XP.ToString());
            }
            
            if (challenge.Rewards.FuelRefill)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(fuelRefillIcon, "1");
            }
            
            foreach (CorePart corePart in challenge.Rewards.CoreParts)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(corePart.Icon, "1");
            }
            
            foreach (SubPart subPart in challenge.Rewards.SubParts)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(subPart.Icon, "1");
            }
            
            foreach (BlueprintReward blueprintReward in challenge.Rewards.Blueprints)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(WarehouseManager.Instance.AllCarData[blueprintReward.CarIndex].Icon, blueprintReward.Blueprints.ToString());
            }
            
            foreach (Unlockable unlockableReward in challenge.Rewards.Unlockables)
            {
                ChallengeRewardUI instance = rewardPrefab.gameObject.GetSpareOrCreate<ChallengeRewardUI>(rewardHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(unlockableReward.Icon, "1");
            }
        }

        public void OnClickClaimButton()
        {
            CoroutineHelper.Instance.StartCoroutine(challenge.Rewards.GiveRewards());
            if (!isUnclaimedChallenge)
                challenge.SetClaimed(true);
            else
                OnClaimUnclaimedChallenge();

            UpdateClaimButton();
            
            PanelManager.GetPanel<ChallengesPanel>().Header.UpdateDailyChallengeNotification();
            PanelManager.GetPanel<ChallengesPanel>().Header.UpdateWeeklyChallengeNotification();
        }
        
        private void OnClaimUnclaimedChallenge()
        {
            gameObject.SetActive(false);
            
            //remove from unclaimed challenges
            challengeManager.RemoveUnclaimedChallenge(challengeManager.ChallengePool.IndexOfItem(challenge));
        }

        private void UpdateDescriptionLabel()
        {
            descriptionLabel.text = challenge.Description;
        }

        private void UpdateDescriptionIcon()
        {
            icon.sprite = challenge.Icon;
        }

        private void UpdateProgressLabel()
        {
            if (isUnclaimedChallenge)
            {
                progressLabel.text = "100%";
                return;
            }
            
            ChallengeTracker.Listener listener = challenge.Tracker.GetListener(challenge.UniqueID);
            int progressAsPercent = Mathf.RoundToInt(listener.Progress * 100);
            progressLabel.text = $"{progressAsPercent}%";
        }
        
        private void UpdateClaimButton()
        {
            if (challenge.IsClaimed)
            {
                descriptionLabel.fontStyle = FontStyles.Strikethrough;
                claimButton.gameObject.SetActive(false);
                notificationIcon.gameObject.SetActive(false);
                return;
            }

            claimButton.gameObject.SetActive(true);
            descriptionLabel.fontStyle = FontStyles.Normal;

            if (isUnclaimedChallenge)
            {
                claimButton.interactable = true;
                notificationIcon.gameObject.SetActive(true);
            }
            else
            {
                ChallengeTracker.Listener challengeListener = challenge.Tracker.GetListener(challenge.UniqueID);
                claimButton.interactable = challengeListener.IsComplete;
                notificationIcon.gameObject.SetActive(challengeListener.IsComplete);
            }
        }

    }
}
