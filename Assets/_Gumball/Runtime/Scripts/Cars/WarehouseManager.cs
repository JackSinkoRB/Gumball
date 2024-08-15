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

        [SerializeField] private List<WarehouseCarData> allCarData = new();

        public delegate void CarChangedDelegate(AICar newCar);
        public event CarChangedDelegate onCurrentCarChanged;

        public AICar CurrentCar { get; private set; }
        public List<WarehouseCarData> AllCarData => allCarData;
        
        public int SavedCarIndex
        {
            get => DataManager.Warehouse.Get("CurrentCar.Index", 0);
            private set => DataManager.Warehouse.Set("CurrentCar.Index", value);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            foreach (WarehouseCarData carData in allCarData)
            {
                carData.OnValidate();
            }
#endif
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
        
        public IEnumerator SwapCurrentCar(int carIndex, Action onComplete = null)
        {
            if (SavedCarIndex == carIndex)
                yield break; //already selected
                
            Destroy(CurrentCar.gameObject);
            
            yield return SpawnCar(carIndex, 
                CurrentCar.transform.position, 
                CurrentCar.transform.rotation,
                SetCurrentCar);
            
            onComplete?.Invoke();
        }
        
        public IEnumerator SpawnSavedCar(Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            yield return SpawnCar(SavedCarIndex, position, rotation, onComplete);
        }

        public IEnumerator SpawnCar(int index, Vector3 position, Quaternion rotation, Action<AICar> onComplete = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            AssetReferenceGameObject assetReference = allCarData[index].CarPrefabReference;
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
