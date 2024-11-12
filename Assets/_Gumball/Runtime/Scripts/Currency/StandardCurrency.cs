using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class StandardCurrency : Currency
    {
        
        protected override CurrencyType CurrencyType => CurrencyType.STANDARD;
        public override int StartingFunds => 500;
        
    }
}
