using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class LevelUpPanel : AnimatedPanel
    {

        [SerializeField] private GameObject premiumCurrencyReward;
        [SerializeField] private TextMeshProUGUI premiumCurrencyLabel;

        public void Populate(PlayerLevel newLevel)
        {
            //populate premium currency:
            if (newLevel.Rewards.PremiumCurrency > 0)
            {
                premiumCurrencyReward.gameObject.SetActive(true);
                premiumCurrencyLabel.text = newLevel.Rewards.PremiumCurrency.ToString();
            }
            else
            {
                premiumCurrencyReward.gameObject.SetActive(false);
            }
        }

    }
}