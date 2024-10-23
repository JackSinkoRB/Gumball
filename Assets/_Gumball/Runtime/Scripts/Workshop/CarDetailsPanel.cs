using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class CarDetailsPanel : AnimatedPanel
    {

        [SerializeField] private Material blackUnlitMaterial;
        [SerializeField] private TextMeshProUGUI makeLabel;
        [SerializeField] private TextMeshProUGUI modelLabel;

        [SerializeField, ReadOnly] private CarOptionUI selectedOption;

        private AICar carInstance;

        public void Initialise(CarOptionUI selectedOption)
        {
            this.selectedOption = selectedOption;
            
            if (carInstance == null)
            {
                //disable the current car
                WarehouseManager.Instance.CurrentCar.gameObject.SetActive(false);
            }
            else
            {
                //destroy previous instance
                Destroy(carInstance.gameObject);
            }

            //spawn the car
            CoroutineHelper.Instance.StartCoroutine(
                WarehouseManager.Instance.SpawnCar(selectedOption.CarIndex, WarehouseManager.Instance.CurrentCar.transform.position, 
                WarehouseManager.Instance.CurrentCar.transform.rotation, OnCarSpawned));
            
            makeLabel.text = selectedOption.CarData.MakeDisplayName;
            modelLabel.text = selectedOption.CarData.DisplayName;

            //typeLabel.text = selectedOption.CarData.CarType.ToString();
        }

        public void OnClickBackButton()
        {
            Hide();
            PanelManager.GetPanel<SwapCarPanel>().Show();
        }
        
        public void OnClickSelectButton()
        {
            Hide();
            
            PanelManager.GetPanel<WarehousePanel>().Show();
            PanelManager.GetPanel<PlayerStatsPanel>().Show();

            CoroutineHelper.Instance.StartCoroutine(WarehouseManager.Instance.SwapCurrentCar(selectedOption.CarIndex));
        }
        
        private void OnCarSpawned(AICar instance)
        {
            carInstance = instance;
            
            if (!selectedOption.CarData.IsUnlocked)
                SetCarBlack();
                
            WarehouseSceneManager.Instance.LockedCarIcon.gameObject.SetActive(!selectedOption.CarData.IsUnlocked);
        }

        private void SetCarBlack()
        {
            //set mesh materials black
            foreach (MeshRenderer meshRenderer in carInstance.transform.GetComponentsInAllChildren<MeshRenderer>())
            {
                Material[] materials = meshRenderer.materials;
                for (int index = 0; index < meshRenderer.materials.Length; index++)
                    materials[index] = blackUnlitMaterial;
                meshRenderer.materials = materials;
            }
        }

    }
}
