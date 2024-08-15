using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gumball
{
    public class SwapCarPanel : AnimatedPanel
    {

        [SerializeField] private CarOptionUI carOptionPrefab;
        [SerializeField] private Transform carOptionHolder;

        [SerializeField, ReadOnly] private CarOptionUI selectedOption;
        
        protected override void OnShow()
        {
            base.OnShow();

            Populate();
        }

        public void OnClickCancelButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
            PanelManager.GetPanel<PlayerStatsPanel>().Show();
        }

        public void SelectCarOption(CarOptionUI option)
        {
            selectedOption = option;
        }

        private void Populate()
        {
            foreach (Transform child in carOptionHolder)
                child.gameObject.Pool();
            
            foreach (WarehouseCarData car in WarehouseManager.Instance.AllCarData)
            {
                CarOptionUI carOptionInstance = carOptionPrefab.gameObject.GetSpareOrCreate<CarOptionUI>(carOptionHolder);
                carOptionInstance.Initialise(car);
            }
        }
        
    }
}