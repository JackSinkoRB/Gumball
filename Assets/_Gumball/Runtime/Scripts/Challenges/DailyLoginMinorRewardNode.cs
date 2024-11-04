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
    public class DailyLoginMinorRewardNode : MonoBehaviour
    {

        [Serializable]
        public struct ColourCodes
        {
            [SerializeField] private GlobalColourPalette.ColourCode locked;
            [SerializeField] private GlobalColourPalette.ColourCode unlocked;
            [SerializeField] private GlobalColourPalette.ColourCode claimed;
            
            public Color LockedColor => GlobalColourPalette.Instance.GetGlobalColor(locked);
            public Color UnlockedColor => GlobalColourPalette.Instance.GetGlobalColor(unlocked);
            public Color ClaimedColor => GlobalColourPalette.Instance.GetGlobalColor(claimed);
        }
        
        [SerializeField] private Image topBackground;
        [SerializeField] private Image bottomBackground;
        [SerializeField] private Image circle;
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

        private MultiImageButton button => GetComponent<MultiImageButton>();
        
        public void Initialise(int dayNumber, MinorDailyLoginReward reward)
        {
            dayLabel.text = $"Day {dayNumber}";
            
            SetReward(reward);
        }

        private void SetReward(MinorDailyLoginReward reward)
        {
            this.reward = reward;
            
            standardCurrencyLabel.text = $"+{reward.StandardCurrencyReward}";
            
            if (!reward.IsReady)
            {
                //locked
                button.interactable = false;
                
                bottomBackground.color = backgroundColourCodes.LockedColor;
                topBackground.color = backgroundColourCodes.LockedColor;
                circle.color = circleColourCodes.LockedColor;
                icon.color = iconColourCodes.LockedColor;

                icon.sprite = lockedIcon;
            }
            else
            if (reward.IsReady && !reward.IsClaimed)
            {
                //unlocked
                button.interactable = true;
                
                bottomBackground.color = backgroundColourCodes.UnlockedColor;
                topBackground.color = backgroundColourCodes.UnlockedColor;
                circle.color = circleColourCodes.UnlockedColor;
                icon.color = iconColourCodes.UnlockedColor;
                
                icon.sprite = unlockedIcon;
            }
            else
            if (reward.IsReady && reward.IsClaimed)
            {
                //claimed
                button.interactable = false;
                
                bottomBackground.color = backgroundColourCodes.ClaimedColor;
                topBackground.color = backgroundColourCodes.ClaimedColor;
                circle.color = circleColourCodes.ClaimedColor;
                icon.color = iconColourCodes.ClaimedColor;
                
                icon.sprite = unlockedIcon;
            }
        }

    }
}
