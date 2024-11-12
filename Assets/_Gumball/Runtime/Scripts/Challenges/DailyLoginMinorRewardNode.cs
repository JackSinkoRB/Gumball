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
    public class DailyLoginMinorRewardNode : DailyLoginRewardNode
    {

        [SerializeField] private Image topBackground;
        [SerializeField] private Image bottomBackground;
        [SerializeField] private Image circle;
        [SerializeField] private Transform notificationIcon;
        [Space(5)]
        [SerializeField] private TextMeshProUGUI dayLabel;
        [SerializeField] private TextMeshProUGUI standardCurrencyLabel;
        [Space(5)]
        [SerializeField] private Image icon;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private Sprite unlockedIcon;
        [Space(5)]
        [SerializeField] private DailyLoginNodeColourCodes backgroundColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes circleColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes iconColourCodes;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private MinorDailyLoginReward reward;
        [SerializeField, ReadOnly] private int dayNumber;

        private MultiImageButton button => GetComponent<MultiImageButton>();
        
        public void Initialise(int dayNumber, MinorDailyLoginReward reward)
        {
            this.dayNumber = dayNumber;
            this.reward = reward;
            
            dayLabel.text = $"Day {dayNumber}";

            button.interactable = DailyLoginManager.Instance.IsDayReady(dayNumber);
            
            SetReward();
        }
        
        protected override void GiveRewards()
        {
            //give standard currency
            if (reward.StandardCurrencyReward > 0)
            {
                PanelManager.GetPanel<VignetteBackgroundPanel>().Show();
                RewardManager.GiveStandardCurrency(reward.StandardCurrencyReward);
                PanelManager.GetPanel<RewardPanel>().Show();
            }
        }

        private void SetReward()
        {
            standardCurrencyLabel.text = $"+{reward.StandardCurrencyReward}";
            
            if (DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                //claimed
                button.interactable = false;
                notificationIcon.gameObject.SetActive(false);
                
                bottomBackground.color = backgroundColourCodes.ClaimedColor;
                topBackground.color = backgroundColourCodes.ClaimedColor;
                circle.color = circleColourCodes.ClaimedColor;
                icon.color = iconColourCodes.ClaimedColor;
                
                icon.sprite = unlockedIcon;
            }
            else
            if (DailyLoginManager.Instance.IsDayReady(dayNumber) && !DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                //ready but not claimed
                button.interactable = true;
                notificationIcon.gameObject.SetActive(true);

                bottomBackground.color = backgroundColourCodes.UnlockedColor;
                topBackground.color = backgroundColourCodes.UnlockedColor;
                circle.color = circleColourCodes.UnlockedColor;
                icon.color = iconColourCodes.UnlockedColor;
                
                icon.sprite = unlockedIcon;
            }
            else
            if (!DailyLoginManager.Instance.IsDayReady(dayNumber))
            {
                //locked
                button.interactable = false;
                notificationIcon.gameObject.SetActive(false);

                bottomBackground.color = backgroundColourCodes.LockedColor;
                topBackground.color = backgroundColourCodes.LockedColor;
                circle.color = circleColourCodes.LockedColor;
                icon.color = iconColourCodes.LockedColor;

                icon.sprite = lockedIcon;
            }
        }

    }
}
