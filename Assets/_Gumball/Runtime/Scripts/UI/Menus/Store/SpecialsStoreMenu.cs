using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SpecialsStoreMenu : StoreSubMenu
    {

        [SerializeField] private Transform optionsHolder;
        [SerializeField] private GameObject optionPrefab;

        private bool hasLoadedOptions;
        
        private void OnEnable()
        {
            if (!hasLoadedOptions)
                LoadOptions();
        }

        private void LoadOptions()
        {
            hasLoadedOptions = true;
            
            //instantiate the options using the playfab data
            
        }
    }
}
