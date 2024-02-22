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
        public List<AssetReferenceGameObject> AllCars => allCars;
        
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
        
        public void SetCurrentCar(CarManager car)
        {
            //remove DontDestroyOnLoad() for existing cars:
            if (CurrentCar != null)
                DontDestroyOnLoadUtils.RemoveDontDestroyOnLoad(CurrentCar.transform);
            
            CurrentCar = car;

            //save the values:
            SavedCarIndex = car.CarIndex;
            SavedCarID = car.ID;
            
            onCurrentCarChanged?.Invoke(car);
            
            //set dont destroy
            car.transform.SetParent(null);
            DontDestroyOnLoad(car.gameObject);
        }
        
        public IEnumerator SpawnSavedCar(Vector3 position, Quaternion rotation, Action<CarManager> onComplete = null)
        {
            yield return SpawnCar(SavedCarIndex, SavedCarID, position, rotation, onComplete);
        }

        public IEnumerator SpawnCar(int index, int id, Vector3 position, Quaternion rotation, Action<CarManager> onComplete = null)
        {
            Debug.Log("[DECAL EDITOR TEST] Spawn car 1");
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            AssetReferenceGameObject assetReference = allCars[index];
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;
            Debug.Log("[DECAL EDITOR TEST] Spawn car 2");

            CarManager car = Instantiate(handle.Result, position, rotation).GetComponent<CarManager>();
            car.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            Debug.Log("[DECAL EDITOR TEST] Spawn car 3");

            yield return car.Initialise(index, id);
            Debug.Log("[DECAL EDITOR TEST] Spawn car 4");

            yield return DecalManager.ApplyDecalDataToCar(car);

            onComplete?.Invoke(car);
            onCurrentCarChanged?.Invoke(car);

#if ENABLE_LOGS
            Debug.Log($"Vehicle loading for {CurrentCar.name} took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
        
    }
}
