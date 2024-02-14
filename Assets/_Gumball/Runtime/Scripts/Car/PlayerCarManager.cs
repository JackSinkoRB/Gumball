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
    public class PlayerCarManager : Singleton<PlayerCarManager>
    {

        public delegate void CarChangedDelegate(CarManager newCar);
        public event CarChangedDelegate onCurrentCarChanged;
        
        [SerializeField] private CarData defaultCarData; //TODO: use save data - for now just using some preset data

        public CarManager CurrentCar { get; private set; }

        public IEnumerator SpawnCar(Vector3 position, Quaternion rotation, Action onComplete = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(defaultCarData.assetReference);
            yield return handle;

            CurrentCar = Instantiate(handle.Result, position, rotation, transform).GetComponent<CarManager>();
            CurrentCar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                
            yield return CurrentCar.Customisation.ApplyVehicleChanges(defaultCarData);

            //if chunk manager is running tests, don't apply decals
            DecalManager.ApplyDecalDataToCar(CurrentCar);

            onComplete?.Invoke();
            onCurrentCarChanged?.Invoke(CurrentCar);
            
#if ENABLE_LOGS
            Debug.Log($"Vehicle loading took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
    }
}
