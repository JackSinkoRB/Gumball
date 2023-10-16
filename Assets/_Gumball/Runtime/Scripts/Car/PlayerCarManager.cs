using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class PlayerCarManager : Singleton<PlayerCarManager>
    {

        public delegate void CarChangedDelegate(CarManager newCar);
        public event CarChangedDelegate onCurrentCarChanged;
        
        [SerializeField] private CarData defaultCarData; //TODO: use save data - for now just using some preset data

        public CarManager CurrentCar { get; private set; }

        public IEnumerator SpawnCar(Vector3 position, Vector3 rotation, Action onComplete = null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(defaultCarData.assetReference);
            yield return handle;

            CurrentCar = Instantiate(handle.Result, position, Quaternion.Euler(rotation), transform).GetComponent<CarManager>();
            CurrentCar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                
            yield return CurrentCar.Customisation.ApplyVehicleChanges(defaultCarData);

            CameraController.Instance.SetTarget(CurrentCar.transform);

            onComplete?.Invoke();
            onCurrentCarChanged?.Invoke(CurrentCar);
        }
    }
}
