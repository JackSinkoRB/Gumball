using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class PurchaseData
    {
        
        [SerializeField] private CurrencyType currencyType;
        
        /// <summary>
        /// This should match the ids of the products in your Google/Apple stores.
        /// </summary>
        [SerializeField, ConditionalField(nameof(currencyType), true)]
        private IAPProduct product;
        [SerializeField, ConditionalField(nameof(currencyType)), PositiveValueOnly]
        private int price = 100;
        
        [Header("Sale Item")]
        [SerializeField] private bool isSale;
        [Tooltip("The discount percent to take from the price. For example: 0.2 means take 20% off the original price. For real items, this is added to the localised price to 'fake' a sale.")]
        [SerializeField, ConditionalField(nameof(isSale)), Range(0, 1)]
        private float discountPercent = 0.2f;

        public CurrencyType CurrencyType => currencyType;
        public IAPProduct Product => product;
        public int Price => price;
        public bool IsSale => isSale;
        public float DiscountPercent => discountPercent;

    }
}
