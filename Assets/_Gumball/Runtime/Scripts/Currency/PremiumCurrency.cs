using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class PremiumCurrency : Currency
    {
        
        protected override CurrencyType CurrencyType => CurrencyType.PREMIUM;
        public override int StartingFunds => 0;
        
    }
}
