using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Gumball
{
    public static class Cheats
    {
        
#if UNITY_EDITOR
        [MenuItem("Gumball/Cheats/Add 10000 Standard Currency")]
#endif
        private static void Add10000StandardCurrency()
        {
            Currency.Standard.AddFunds(10000);
        }
        
    }
}