using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class PartsWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private GridLayoutWithScreenSize gridLayout;
        [SerializeField] private PartsOption partsOptionPrefab;

        protected override void OnShow()
        {
            base.OnShow();

            PopulateScrollWithBodyKits();
        }

        private void PopulateScrollWithBodyKits()
        {
            //TODO: handle if there is more than 1 group
            
            CarPartManager partManager = WarehouseManager.Instance.CurrentCar.CarPartManager;
            if (partManager == null || partManager.CarPartGroups.Length == 0)
            {
                PopulateGrid(null);
                return;
            }
            
            CarPartGroup bodyKitGroup = partManager.CarPartGroups[0];
            PopulateGrid(bodyKitGroup);
        }

        private void PopulateGrid(CarPartGroup group)
        {
            foreach (Transform child in gridLayout.transform)
                child.gameObject.Pool();
            
            if (group != null)
            {
                PartsOption currentPart = null;
                for (int index = 0; index < group.CarParts.Length; index++)
                {
                    CarPart part = group.CarParts[index];
                    
                    PartsOption instance = partsOptionPrefab.gameObject.GetSpareOrCreate<PartsOption>(gridLayout.transform);
                    instance.Initialise(part, group);
                    instance.transform.SetAsLastSibling();

                    if (group.CurrentPartIndex == index)
                        currentPart = instance;
                }
                
                if (currentPart != null)
                    currentPart.OnSelect();
            }

            this.PerformAtEndOfFrame(() => gridLayout.Resize());
        }

    }
}
