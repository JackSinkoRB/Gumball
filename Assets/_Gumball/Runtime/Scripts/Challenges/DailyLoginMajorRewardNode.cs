using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(MultiImageButton))]
    public class DailyLoginMajorRewardNode : MonoBehaviour
    {

        [SerializeField] private Image background;
        [SerializeField] private Image topBackground;
        [SerializeField] private Image bottomBackground;
        [SerializeField] private Image dayCircle;
        [SerializeField] private Image standardCurrencyBackground;
        [SerializeField] private Image standardCurrencyIcon;
        [SerializeField] private Sprite standardCurrencySprite;
        [SerializeField] private Sprite standardCurrencyLockedSprite;
        [Space(5)]
        [SerializeField] private TextMeshProUGUI dayLabel;
        [SerializeField] private TextMeshProUGUI standardCurrencyLabel;
        [Space(5)]
        [SerializeField] private DailyLoginMajorRewardNodeRewardUI rewardUIPrefab;
        [SerializeField] private Transform rewardUIHolder;
        [Space(5)]
        [SerializeField] private DailyLoginNodeColourCodes backgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes topBackgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes bottomBackgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes dayCircleColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyLabelColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyBackgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyIconColourCodes;
        [Space(5)]
        [SerializeField] private Sprite premiumCurrencyIcon;
        [SerializeField] private Sprite xpIcon;
        [SerializeField] private Sprite fuelRefillIcon;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private int dayNumber;
        [SerializeField, ReadOnly] private MajorDailyLoginReward reward;

        private MultiImageButton button => GetComponent<MultiImageButton>();
        
        public void Initialise(int dayNumber, MajorDailyLoginReward reward)
        {
            this.dayNumber = dayNumber;
            this.reward = reward;
            
            dayLabel.text = $"Day {dayNumber}";
            
            SetReward();
        }
        
        private void SetReward()
        {
            standardCurrencyLabel.text = $"+{reward.Rewards.StandardCurrency}";

            PopulateRewards();
            
            if (!DailyLoginManager.Instance.IsDayReady(dayNumber))
            {
                //locked
                button.interactable = false;
                
                topBackground.gameObject.SetActive(false);
                bottomBackground.gameObject.SetActive(false);

                background.color = backgroundColourCodes.LockedColor;
                topBackground.color = topBackgroundColourCodes.LockedColor;
                bottomBackground.color = bottomBackgroundColourCodes.LockedColor;
                dayCircle.color = dayCircleColourCodes.LockedColor;
                standardCurrencyBackground.color = standardCurrencyBackgroundColourCodes.LockedColor;
                standardCurrencyIcon.color = standardCurrencyIconColourCodes.LockedColor;
                standardCurrencyLabel.color = standardCurrencyLabelColourCodes.LockedColor;

                standardCurrencyIcon.sprite = standardCurrencyLockedSprite;
            }
            else
            if (DailyLoginManager.Instance.IsDayReady(dayNumber) && !DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                //unlocked
                button.interactable = true;
                
                topBackground.gameObject.SetActive(true);
                bottomBackground.gameObject.SetActive(true);

                background.color = backgroundColourCodes.UnlockedColor;
                topBackground.color = topBackgroundColourCodes.UnlockedColor;
                bottomBackground.color = bottomBackgroundColourCodes.UnlockedColor;
                dayCircle.color = dayCircleColourCodes.UnlockedColor;
                standardCurrencyBackground.color = standardCurrencyBackgroundColourCodes.UnlockedColor;
                standardCurrencyIcon.color = standardCurrencyIconColourCodes.UnlockedColor;
                standardCurrencyLabel.color = standardCurrencyLabelColourCodes.UnlockedColor;
                
                standardCurrencyIcon.sprite = standardCurrencySprite;
            }
            else
            if (DailyLoginManager.Instance.IsDayReady(dayNumber) && DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                //claimed
                button.interactable = false;
                
                topBackground.gameObject.SetActive(true);
                bottomBackground.gameObject.SetActive(true);

                background.color = backgroundColourCodes.ClaimedColor;
                topBackground.color = topBackgroundColourCodes.ClaimedColor;
                bottomBackground.color = bottomBackgroundColourCodes.ClaimedColor;
                dayCircle.color = dayCircleColourCodes.ClaimedColor;
                standardCurrencyBackground.color = standardCurrencyBackgroundColourCodes.ClaimedColor;
                standardCurrencyIcon.color = standardCurrencyIconColourCodes.ClaimedColor;
                standardCurrencyLabel.color = standardCurrencyLabelColourCodes.ClaimedColor;
                
                standardCurrencyIcon.sprite = standardCurrencySprite;
            }
        }

        private void PopulateRewards()
        {
            foreach (Transform child in rewardUIHolder)
                child.gameObject.Pool();

            if (reward.Rewards.PremiumCurrency > 0)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, premiumCurrencyIcon, reward.Rewards.PremiumCurrency);
            }
            
            if (reward.Rewards.XP > 0)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, xpIcon, reward.Rewards.XP);
            }
            
            if (reward.Rewards.FuelRefill)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, fuelRefillIcon, 1);
            }
            
            foreach (CorePart corePart in reward.Rewards.CoreParts)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, corePart.Icon, 1);
            }
            
            foreach (SubPart subPart in reward.Rewards.SubParts)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, subPart.Icon, 1);
            }
            
            foreach (BlueprintReward blueprintReward in reward.Rewards.Blueprints)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, WarehouseManager.Instance.AllCarData[blueprintReward.CarIndex].Icon, blueprintReward.Blueprints);
            }
            
            foreach (Unlockable unlockableReward in reward.Rewards.Unlockables)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>(rewardUIHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(dayNumber, unlockableReward.Icon, 1);
            }
        }
        
    }
}
