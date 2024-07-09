using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Gumball
{
    [Serializable]
    public class IAPProduct
    {
        
        [SerializeField] private string productID = "com.RoadburnGames.Gumball.<SOME_ID>";
        [SerializeField] private ProductType type = ProductType.NonConsumable;

        public string ProductID => productID;
        public ProductType Type => type;

    }
}
