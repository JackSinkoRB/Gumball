using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class LevelUpPanel : AnimatedPanel
    {

        [SerializeField] private Transform fuelRefillUI;
        [SerializeField] private Transform premiumCurrencyUI;
        [SerializeField] private TextMeshProUGUI premiumCurrencyLabel;

        public void Populate(PlayerLevel newLevel)
        {
            //fuel refill
            fuelRefillUI.gameObject.SetActive(newLevel.Rewards.FuelRefill);

            //premium currency:
            premiumCurrencyUI.gameObject.SetActive(newLevel.Rewards.PremiumCurrency > 0);
            premiumCurrencyLabel.text = newLevel.Rewards.PremiumCurrency.ToString();
        }

    }
}