using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

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

            PanelManager.GetPanel<PaintStripeBackgroundPanel>().Show();
            
            Populate();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            if (PanelManager.PanelExists<PaintStripeBackgroundPanel>())
                PanelManager.GetPanel<PaintStripeBackgroundPanel>().Hide();
        }

        public void OnClickCancelButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
            PanelManager.GetPanel<PlayerStatsPanel>().Show();
        }

        public void OnClickInfoButton()
        {
            Hide();
            
            PanelManager.GetPanel<CarDetailsPanel>().Show();
            PanelManager.GetPanel<CarDetailsPanel>().Initialise(selectedOption);
        }
        
        public void SelectCarOption(CarOptionUI option)
        {
            if (option == selectedOption)
                return; //already selected
            
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            selectedOption.OnSelect();
        }

        private void Populate()
        {
            foreach (Transform child in carOptionHolder)
                child.gameObject.Pool();

            foreach (WarehouseCarData car in WarehouseManager.Instance.AllCarData)
            {
                CarOptionUI carOptionInstance = carOptionPrefab.gameObject.GetSpareOrCreate<CarOptionUI>(carOptionHolder);
                carOptionInstance.Initialise(car);

                if (car.GUID.Equals(WarehouseManager.Instance.SavedCarGUID))
                    SelectCarOption(carOptionInstance);
            }
        }

    }
}