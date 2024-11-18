using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Warehouse Manager")]
    public class WarehouseManager : SingletonScriptable<WarehouseManager>
    {
        
        private static readonly GenericDictionary<PerformanceRatingCalculator.Component, int> maxPerformanceRatingValuesCached = new();

        [SerializeField] private string defaultCarGUID;
        [SerializeField] private List<WarehouseCarData> allCarData = new();
        
        public delegate void CarChangedDelegate(AICar newCar);
        public event CarChangedDelegate onCurrentCarChanged;

        public AICar CurrentCar { get; private set; }
        public List<WarehouseCarData> AllCarData => allCarData;

        [SerializeField, ReadOnly] private GenericDictionary<string, int> lookupByGUID = new();
        
        public WarehouseCarData GetCarDataFromGUID(string guid)
        {
            if (!lookupByGUID.ContainsKey(guid))
                throw new NullReferenceException($"There is no car data matching the GUID {guid}");

            int carIndex = lookupByGUID[guid];
            return allCarData[carIndex];
        }
        
        public string SavedCarGUID
        {
            get => DataManager.Warehouse.Get("CurrentCar.GUID", defaultCarGUID);
            private set => DataManager.Warehouse.Set("CurrentCar.GUID", value);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            maxPerformanceRatingValuesCached.Clear();
        }
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            UpdateCachedData();
#endif
        }
        
#if UNITY_EDITOR
        public void UpdateCachedData()
        {
            lookupByGUID.Clear();

            HashSet<string> duplicateCheck = new();
            for (int carIndex = 0; carIndex < allCarData.Count; carIndex++)
            {
                WarehouseCarData carData = allCarData[carIndex];
                carData.CacheCarData();

                if (duplicateCheck.Contains(carData.GUID))
                    carData.AssignNewGUID();
                
                lookupByGUID[carData.GUID] = carIndex;
                duplicateCheck.Add(carData.GUID);
            }
        }
#endif

        public void SetCurrentCar(AICar car)
        {
            //remove DontDestroyOnLoad() for existing cars:
            if (CurrentCar != null)
                DontDestroyOnLoadUtils.RemoveDontDestroyOnLoad(CurrentCar.transform);
            
            CurrentCar = car;

            //save the values:
            SavedCarGUID = car.CarGUID;
            
            onCurrentCarChanged?.Invoke(car);
            
            //set dont destroy
            car.transform.SetParent(null);
            DontDestroyOnLoad(car.gameObject);
        }
        
        public IEnumerator SwapCurrentCar(string carGUID, Action onComplete = null)
        {
            if (SavedCarGUID.Equals(carGUID))
                yield break; //already selected
                
            Destroy(CurrentCar.gameObject);
            
            yield return SpawnCar(carGUID, 
                CurrentCar.transform.position, 
                CurrentCar.transform.rotation,
                SetCurrentCar);
            
            onComplete?.Invoke();
        }
        
        public void SwapCurrentCar(AICar car)
        {
            if (SavedCarGUID.Equals(car.CarGUID))
                return; //already selected
                
            if (CurrentCar != null)
                Destroy(CurrentCar.gameObject);
            
            SetCurrentCar(car);
        }
        
        public IEnumerator SpawnSavedCar(Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            yield return SpawnCar(SavedCarGUID, position, rotation, onComplete);
        }

        public IEnumerator SpawnCar(string carGUID, Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            AssetReferenceGameObject assetReference = GetCarDataFromGUID(carGUID).CarPrefabReference;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;
            
            AICar car = Instantiate(handle.Result, position, rotation).GetComponent<AICar>();
            car.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            car.gameObject.SetActive(false); //disable until complete
            
            car.InitialiseAsPlayer(carGUID);
            car.SetGrounded();
            
            yield return DecalManager.ApplyDecalDataToCar(car);
            
            onComplete?.Invoke(car);
            
            car.gameObject.SetActive(true);

#if ENABLE_LOGS
            Debug.Log($"Vehicle loading for {car.name} took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }

        public int GetMaxRating(PerformanceRatingCalculator.Component ratingComponent)
        {
            if (!maxPerformanceRatingValuesCached.ContainsKey(ratingComponent))
            {
                maxPerformanceRatingValuesCached[ratingComponent] = CalculateMaxRating(ratingComponent);
                Debug.Log($"Cached max performance rating for {ratingComponent.ToString()} as {maxPerformanceRatingValuesCached[ratingComponent]}.");
            }

            return maxPerformanceRatingValuesCached[ratingComponent];
        }
        
        private int CalculateMaxRating(PerformanceRatingCalculator.Component ratingComponent)
        {
            int maxRating = 0;
            for (int carIndex = 0; carIndex < allCarData.Count; carIndex++)
            {
                WarehouseCarData carData = allCarData[carIndex];
                PerformanceRatingCalculator calculator = PerformanceRatingCalculator.GetCalculator(carData.PerformanceSettings, new CarPerformanceProfile(1,1,1,1));
                
                int ratingComponentValue = calculator.GetRating(ratingComponent);
                if (ratingComponentValue > maxRating)
                    maxRating = ratingComponentValue;
            }

            return maxRating;
        }
        
    }
}
