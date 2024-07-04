using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class PremiumCurrencyUI : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI currencyLabel;
        
        private void OnEnable()
        {
            RefreshCurrencyLabel();

            Currency.Premium.onFundsChange += OnFundsChange;
        }
        
        private void OnDisable()
        {
            Currency.Premium.onFundsChange -= OnFundsChange;
        }
        
        private void OnFundsChange(int previousFunds, int newFunds)
        {
            RefreshCurrencyLabel();
        }
        
        private void RefreshCurrencyLabel()
        {
            currencyLabel.text = $"{Currency.Premium.Funds}";
        }
        
    }
}
