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
        [SerializeField] private Button selectButton;
        
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

        public void OnClickSelectButton()
        {
            Hide();
            PanelManager.GetPanel<WarehousePanel>().Show();
            PanelManager.GetPanel<PlayerStatsPanel>().Show();

            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SwapCurrentCar(selectedOption.CarIndex));
        }
        
        public void SelectCarOption(CarOptionUI option)
        {
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            selectedOption.OnSelect();

            //don't show interactable if already selected
            selectButton.interactable = !option.IsCurrentCar;
        }

        private void Populate()
        {
            foreach (Transform child in carOptionHolder)
                child.gameObject.Pool();

            for (int carIndex = 0; carIndex < WarehouseManager.Instance.AllCarData.Count; carIndex++)
            {
                WarehouseCarData car = WarehouseManager.Instance.AllCarData[carIndex];
                
                CarOptionUI carOptionInstance = carOptionPrefab.gameObject.GetSpareOrCreate<CarOptionUI>(carOptionHolder);
                carOptionInstance.Initialise(car, carIndex);

                if (carIndex == WarehouseManager.Instance.SavedCarIndex)
                    SelectCarOption(carOptionInstance);
            }
        }

    }
}