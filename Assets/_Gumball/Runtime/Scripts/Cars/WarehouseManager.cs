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

        public delegate void CarChangedDelegate(AICar newCar);
        public event CarChangedDelegate onCurrentCarChanged;

        public AICar CurrentCar { get; private set; }
        public List<AssetReferenceGameObject> AllCars => allCars;
        
        public int SavedCarIndex
        {
            get => DataManager.Warehouse.Get("CurrentCar.Index", 0);
            private set => DataManager.Warehouse.Set("CurrentCar.Index", value);
        }
        
        public void SetCurrentCar(AICar car)
        {
            //remove DontDestroyOnLoad() for existing cars:
            if (CurrentCar != null)
                DontDestroyOnLoadUtils.RemoveDontDestroyOnLoad(CurrentCar.transform);
            
            CurrentCar = car;

            //save the values:
            SavedCarIndex = car.CarIndex;
            
            onCurrentCarChanged?.Invoke(car);
            
            //set dont destroy
            car.transform.SetParent(null);
            DontDestroyOnLoad(car.gameObject);
        }
        
        public IEnumerator SpawnSavedCar(Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            yield return SpawnCar(SavedCarIndex, position, rotation, onComplete);
        }

        public IEnumerator SpawnCar(int index, Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            AssetReferenceGameObject assetReference = allCars[index];
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            yield return handle;
            
            AICar car = Instantiate(handle.Result, position, rotation).GetComponent<AICar>();
            car.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            car.InitialiseAsPlayer(index);
            car.SetGrounded();
            
            yield return DecalManager.ApplyDecalDataToCar(car);
            
            onComplete?.Invoke(car);
            
#if ENABLE_LOGS
            Debug.Log($"Vehicle loading for {CurrentCar.name} took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
        
    }
}
