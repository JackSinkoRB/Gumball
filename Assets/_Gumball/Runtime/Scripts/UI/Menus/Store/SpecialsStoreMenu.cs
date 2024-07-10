using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SpecialsStoreMenu : StoreSubMenu
    {

        private bool isInitialised;
        
        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            //TODO: keep list of purchase buttons in this menu - in StorePurchaseButton OnValidate check if it's got the special menu in the parent, then add it to the list
            foreach (StorePurchaseButton purchaseButton in transform.GetComponentsInAllChildren<StorePurchaseButton>())
            {
                //check in PlayFab title data if it's enabled, otherwise deactivate it
                const string key = "Store.Specials";
                bool isShown = purchaseButton.UniqueID != null && PlayFabManager.HasKey(key) && PlayFabManager.Get<List<String>>(key).Contains(purchaseButton.UniqueID);
                
                Transform storeOption = purchaseButton.transform.parent;
                storeOption.gameObject.SetActive(isShown);
            }
        }
    }
}
