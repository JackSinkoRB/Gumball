using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using UnityEngine;

namespace Gumball
{
    public class WarehousePanel : AnimatedPanel
    {

        [SerializeField] private MagneticScroll magneticScroll;

        public void OnClickBackButton()
        {
            MainSceneManager.LoadMainScene();
        }
        
        protected override void OnShow()
        {
            base.OnShow();

            PopulateMagneticScroll();
        }

        private void PopulateMagneticScroll()
        {
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            for (int index = 0; index < WarehouseSceneManager.Instance.CarSlots.Length; index++)
            {
                int finalIndex = index;

                if (index >= WarehouseManager.Instance.AllCars.Count)
                    break; //only show slots for maximum amount of cars - TODO: should only show slots that player's cars are in
                
                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onSelect += () => WarehouseSceneManager.Instance.SelectSlot(finalIndex);

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems);
        }
        
    }
}
