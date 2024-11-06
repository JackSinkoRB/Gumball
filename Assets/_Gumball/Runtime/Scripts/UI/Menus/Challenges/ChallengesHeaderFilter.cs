using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ChallengesHeaderFilter : MonoBehaviour
    {

        [SerializeField] private Button[] buttons;
        [SerializeField] private Color selectedFilterButtonColor = Color.white;
        [SerializeField] private Color deselectedFilterButtonColor = Color.grey;
        [SerializeField] private Image selector;
        [SerializeField] private Vector2 selectorPadding;
        [SerializeField] private Transform dailyLoginNotification;

        private void OnEnable()
        {
            UpdateDailyLoginNotification();
        }
        
        public void Select(Button categoryButton)
        {
            selector.rectTransform.anchoredPosition = categoryButton.GetComponent<RectTransform>().anchoredPosition;
            selector.rectTransform.sizeDelta = categoryButton.GetComponent<RectTransform>().sizeDelta + selectorPadding;

            foreach (Button button in buttons)
            {
                foreach (Graphic graphic in button.transform.GetComponentsInAllChildren<Graphic>())
                    graphic.color = button == categoryButton  ? selectedFilterButtonColor : deselectedFilterButtonColor;
            }
        }

        public void UpdateDailyLoginNotification()
        {
            int currentDayNumber = DailyLoginManager.Instance.GetCurrentDayNumber();
            bool isDayWaiting = DailyLoginManager.Instance.IsDayReady(currentDayNumber) && !DailyLoginManager.Instance.IsDayClaimed(currentDayNumber);
            dailyLoginNotification.gameObject.SetActive(isDayWaiting);
        }
        
    }
}
