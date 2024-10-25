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
        [SerializeField] private MultiImageButton selectButton;
        [SerializeField] private StorePurchaseButton purchaseButton;
        
        [SerializeField] private AutosizeTextMeshPro ratingLabel;
        [SerializeField] private Transform levelHolder;
        [SerializeField] private CarLevelUI carLevelUI;
        [SerializeField] private CarBlueprintsUI blueprintUI;
        [SerializeField] private Transform openBlueprints;

        [SerializeField] private PerformanceRatingSlider maxSpeedSlider;
        [SerializeField] private PerformanceRatingSlider accelerationSlider;
        [SerializeField] private PerformanceRatingSlider handlingSlider;
        [SerializeField] private PerformanceRatingSlider nosSlider;
        
        [SerializeField] private OpenBlueprintOption openBlueprintOptionPrefab;
        [SerializeField] private Transform openBlueprintOptionHolder;
        
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
            
            purchaseButton.gameObject.SetActive(!selectedOption.CarData.IsUnlocked);
            selectButton.gameObject.SetActive(selectedOption.CarData.IsUnlocked);

            purchaseButton.SetPurchaseData(selectedOption.CarData.CostToUnlock);

            levelHolder.gameObject.SetActive(selectedOption.CarData.IsUnlocked);
            carLevelUI.SetCarIndex(selectedOption.CarIndex);
            
            blueprintUI.SetCarIndex(selectedOption.CarIndex);

            openBlueprints.gameObject.SetActive(!selectedOption.CarData.IsUnlocked);
            
            //check if enough blueprints
            if (!selectedOption.CarData.IsUnlocked)
            {
                int blueprints = BlueprintManager.Instance.GetBlueprints(selectedOption.CarIndex);
                int requiredBlueprints = BlueprintManager.Instance.GetBlueprintsRequiredForLevel(selectedOption.CarData.StartingLevelIndex + 1);
                purchaseButton.GetComponent<MultiImageButton>().interactable = blueprints > requiredBlueprints;
                
                PopulateOpenBlueprints();
            }
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

            WarehouseManager.Instance.SwapCurrentCar(carInstance);
        }

        public void OnSuccessfulPurchase()
        {
            selectedOption.CarData.Unlock();

            //refresh
            Initialise(selectedOption);
        }
        
        private void OnCarSpawned(AICar instance)
        {
            carInstance = instance;
            
            if (!selectedOption.CarData.IsUnlocked)
                SetCarBlack();
                
            WarehouseSceneManager.Instance.LockedCarIcon.gameObject.SetActive(!selectedOption.CarData.IsUnlocked);
            
            //update rating label
            ratingLabel.text = $"{carInstance.CurrentPerformanceRating.TotalRating}";
            this.PerformAtEndOfFrame(ratingLabel.Resize);
            
            CarPerformanceProfile currentProfile = new CarPerformanceProfile(instance.CarIndex);
            maxSpeedSlider.Initialise(selectedOption.CarData.PerformanceSettings, currentProfile);
            accelerationSlider.Initialise(selectedOption.CarData.PerformanceSettings, currentProfile);
            handlingSlider.Initialise(selectedOption.CarData.PerformanceSettings, currentProfile);
            nosSlider.Initialise(selectedOption.CarData.PerformanceSettings, currentProfile);
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

        private void PopulateOpenBlueprints()
        {
            foreach (Transform child in openBlueprintOptionHolder)
                child.gameObject.Pool();
            
            //get the sessions that give the CarIndex blueprint as a reward
            foreach (GameSession session in BlueprintManager.Instance.GetSessionsThatGiveBlueprint(selectedOption.CarIndex))
            {
                OpenBlueprintOption instance = openBlueprintOptionPrefab.gameObject.GetSpareOrCreate<OpenBlueprintOption>(openBlueprintOptionHolder);
                instance.transform.SetAsLastSibling();
                instance.Initialise(session.GetModeIcon(), session.DisplayName);
            }
        }

    }
}
