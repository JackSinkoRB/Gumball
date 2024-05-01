using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class PartsWorkshopMenu : WorkshopSubMenu
    {

        [SerializeField] private MagneticScroll magneticScroll;

        public override void Show()
        {
            base.Show();

            PopulateScrollWithBodyKits();
        }

        private void PopulateScrollWithBodyKits()
        {
            //TODO: handle if there is more than 1 group
            
            CarPartManager partManager = WarehouseManager.Instance.CurrentCar.CarPartManager;
            if (partManager == null || partManager.CarPartGroups.Length == 0)
            {
                PopulateScroll(null);
                return;
            }
            
            CarPartGroup bodyKitGroup = partManager.CarPartGroups[0];
            PopulateScroll(bodyKitGroup);
        }

        private void PopulateScroll(CarPartGroup group)
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();

            if (group != null)
            {
                for (int index = 0; index < group.CarParts.Length; index++)
                {
                    int finalIndex = index;

                    CarPart part = group.CarParts[index];
                    ScrollItem scrollItem = new ScrollItem();
                    scrollItem.onLoad += () =>
                    {
                        PartsScrollIcon partsScrollIcon = (PartsScrollIcon)scrollItem.CurrentIcon;
                        partsScrollIcon.ImageComponent.sprite = part.Icon;
                        partsScrollIcon.DisplayNameLabel.text = part.DisplayName;
                    };

                    scrollItem.onSelect += () =>
                    {
                        group.SetPartActive(finalIndex);
                    };

                    scrollItems.Add(scrollItem);
                }
            }

            magneticScroll.SetItems(scrollItems, group == null ? 0 : group.CurrentPartIndex);
        }

    }
}
