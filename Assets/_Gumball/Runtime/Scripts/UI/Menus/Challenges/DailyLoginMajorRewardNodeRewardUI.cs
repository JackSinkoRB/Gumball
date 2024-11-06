using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DailyLoginMajorRewardNodeRewardUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI amountLabel;
        [SerializeField] private Image amountCircle;
        [SerializeField] private Image icon;
        [Space(5)]
        [SerializeField] private DailyLoginNodeColourCodes circleColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes iconColourCodes;
        [SerializeField] private DailyLoginNodeColourCodes labelColourCodes;

        public void Initialise(int dayNumber, Sprite iconSprite, int amountValue)
        {
            if (amountValue == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            icon.sprite = iconSprite;
            amountLabel.text = $"{amountValue}";
            
            if (!DailyLoginManager.Instance.IsDayReady(dayNumber))
            {
                amountCircle.color = circleColourCodes.LockedColor;
                icon.color = iconColourCodes.LockedColor;
                amountLabel.color = labelColourCodes.LockedColor;
            }
            else 
            if (DailyLoginManager.Instance.IsDayReady(dayNumber) && !DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                amountCircle.color = circleColourCodes.UnlockedColor;
                icon.color = iconColourCodes.UnlockedColor;
                amountLabel.color = labelColourCodes.UnlockedColor;
            }
            else if (DailyLoginManager.Instance.IsDayReady(dayNumber) && DailyLoginManager.Instance.IsDayClaimed(dayNumber))
            {
                amountCircle.color = circleColourCodes.ClaimedColor;
                icon.color = iconColourCodes.ClaimedColor;
                amountLabel.color = labelColourCodes.ClaimedColor;
            }
        }

    }
}
