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
        
        [Tooltip("This should be the product ID reference from the Apple/Google stores.")]
        [SerializeField] private string productID = "<STORE PRODUCT ID>";
        [Tooltip("This should be the same type that the product is in the Apple/Google stores.")]
        [SerializeField] private ProductType type = ProductType.NonConsumable;
        
        public string ProductID => productID;
        public ProductType Type => type;

    }
}
