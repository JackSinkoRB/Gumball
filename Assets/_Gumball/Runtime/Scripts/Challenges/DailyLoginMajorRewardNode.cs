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
        [SerializeField] private DailyLoginNodeColourCodes dayCircleColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyLabelColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyBackgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes standardCurrencyIconColourCodes;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private MajorDailyLoginReward reward;

        private MultiImageButton button => GetComponent<MultiImageButton>();
        
        public void Initialise(int dayNumber, MajorDailyLoginReward reward)
        {
            dayLabel.text = $"Day {dayNumber}";
            
            SetReward(reward);
        }
        
        private void SetReward(MajorDailyLoginReward reward)
        {
            this.reward = reward;
            
            standardCurrencyLabel.text = $"+{reward.Rewards.StandardCurrency}";

            PopulateRewards();
            
            if (!reward.IsReady)
            {
                //locked
                button.interactable = false;
                
                background.color = backgroundColourCodes.LockedColor;
                dayCircle.color = dayCircleColourCodes.LockedColor;
                standardCurrencyBackground.color = standardCurrencyBackgroundColourCodes.LockedColor;
                standardCurrencyIcon.color = standardCurrencyIconColourCodes.LockedColor;
                standardCurrencyLabel.color = standardCurrencyLabelColourCodes.LockedColor;

                standardCurrencyIcon.sprite = standardCurrencyLockedSprite;
            }
            else
            if (reward.IsReady && !reward.IsClaimed)
            {
                //unlocked
                button.interactable = true;
                
                background.color = backgroundColourCodes.UnlockedColor;
                dayCircle.color = dayCircleColourCodes.UnlockedColor;
                standardCurrencyBackground.color = standardCurrencyBackgroundColourCodes.UnlockedColor;
                standardCurrencyIcon.color = standardCurrencyIconColourCodes.UnlockedColor;
                standardCurrencyLabel.color = standardCurrencyLabelColourCodes.UnlockedColor;
                
                standardCurrencyIcon.sprite = standardCurrencySprite;
            }
            else
            if (reward.IsReady && reward.IsClaimed)
            {
                //claimed
                button.interactable = false;
                
                background.color = backgroundColourCodes.ClaimedColor;
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

            foreach (CorePart corePart in reward.Rewards.CoreParts)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>();
                instance.transform.SetAsLastSibling();
                instance.Initialise(reward, corePart.Icon, 1);
            }
            
            foreach (SubPart subPart in reward.Rewards.SubParts)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>();
                instance.transform.SetAsLastSibling();
                instance.Initialise(reward, subPart.Icon, 1);
            }
            
            foreach (BlueprintReward blueprintReward in reward.Rewards.Blueprints)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>();
                instance.transform.SetAsLastSibling();
                instance.Initialise(reward, WarehouseManager.Instance.AllCarData[blueprintReward.CarIndex].Icon, blueprintReward.Blueprints);
            }
            
            foreach (Unlockable unlockableReward in reward.Rewards.Unlockables)
            {
                DailyLoginMajorRewardNodeRewardUI instance = rewardUIPrefab.gameObject.GetSpareOrCreate<DailyLoginMajorRewardNodeRewardUI>();
                instance.transform.SetAsLastSibling();
                instance.Initialise(reward, unlockableReward.Icon, 1);
            }
        }
        
    }
}
