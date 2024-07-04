using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class CurrencyManager
    {

        private static Currency standardInstance;
        private static Currency premiumInstance;

        public static Currency Standard => standardInstance ??= new StandardCurrency();
        public static Currency Premium => premiumInstance ??= new PremiumCurrency();
        
    }
}
