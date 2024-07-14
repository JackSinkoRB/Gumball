using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public abstract class Currency
    {
        
        #region Instances
        private static Currency standardInstance;
        private static Currency premiumInstance;

        public static Currency Standard => standardInstance ??= new StandardCurrency();
        public static Currency Premium => premiumInstance ??= new PremiumCurrency();
        #endregion
        
        protected abstract CurrencyType CurrencyType { get; }
        public abstract int StartingFunds { get; }
        
        public delegate void OnFundsChangeDelegate(int previousFunds, int newFunds);
        public event OnFundsChangeDelegate onFundsChange;
        
        public int Funds
        {
            get => DataManager.Player.Get($"Currency.{CurrencyType}", StartingFunds);
            private set => DataManager.Player.Set($"Currency.{CurrencyType}", value);
        }
        
        public bool HasEnoughFunds(int amount)
        {
            return Funds >= amount;
        }
        
        public void SetFunds(int amount)
        {
            if (amount < 0)
            {
                Debug.LogError("Cannot set funds below 0.");
                return;
            }
            
            int previous = Funds;
            
            if (previous == amount)
                return; // no change
            
            Funds = amount;
            GlobalLoggers.CurrencyLogger.Log($"{CurrencyType} funds modified from {previous} to {amount}.");
            
            onFundsChange?.Invoke(previous, amount);
        }
        
        public void AddFunds(int amount)
        {
            SetFunds(Funds + amount);
        }
        
        public void TakeFunds(int amount)
        {
            SetFunds(Funds - amount);
        }
        
    }
}
