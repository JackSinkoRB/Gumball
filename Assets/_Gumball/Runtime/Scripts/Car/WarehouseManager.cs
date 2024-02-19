using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Warehouse Manager")]
    public class WarehouseManager : SingletonScriptable<WarehouseManager>
    {
        
        [SerializeField] private List<AssetReferenceGameObject> allCars;

        public delegate void CarChangedDelegate(CarManager newCar);
        public event CarChangedDelegate onCurrentCarChanged;

        public CarManager CurrentCar { get; private set; }
        public string CurrentCarSaveKey => $"CarData.{SavedCarIndex}.{SavedCarID}";

        public int SavedCarIndex
        {
            get => DataManager.Warehouse.Get("CurrentCar.Index", 0);
            private set => DataManager.Warehouse.Set("CurrentCar.Index", value);
        }

        public int SavedCarID
        {
            get => DataManager.Warehouse.Get("CurrentCar.ID", 0);
            private set => DataManager.Warehouse.Set("CurrentCar.ID", value);
        }

        /// <summary>
        /// Sets the car as the current/saved car.
        /// </summary>
        public void SelectCar(int index, int id)
        {
            SavedCarIndex = index;
            SavedCarID = id;
        }
        
        public IEnumerator SpawnSavedCar(Vector3 position, Quaternion rotation)
        {
            yield return SpawnCar(SavedCarIndex, SavedCarID, position, rotation);
        }

        public IEnumerator SpawnCar(int index, int id, Vector3 position, Quaternion rotation, Action onComplete = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            AssetReferenceGameObject assetReference = allCars[index];
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;

            CurrentCar = Instantiate(handle.Result, position, rotation).GetComponent<CarManager>();
            CurrentCar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            DontDestroyOnLoad(CurrentCar.gameObject);
            
            yield return CurrentCar.Initialise(index, id);
            
            DecalManager.ApplyDecalDataToCar(CurrentCar);

            onComplete?.Invoke();
            onCurrentCarChanged?.Invoke(CurrentCar);
            
#if ENABLE_LOGS
            Debug.Log($"Vehicle loading for {CurrentCar.name} took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
        
    }
}
