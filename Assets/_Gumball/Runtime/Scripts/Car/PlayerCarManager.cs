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
        
        [SerializeField] private CarData defaultCarData; //TODO: use save data - for now just using some preset data

        public CarController CurrentCar { get; private set; }

        public IEnumerator SpawnCar(Action onComplete = null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(defaultCarData.assetReference);
            yield return handle;
            
            CurrentCar = Instantiate(handle.Result, transform, false).GetComponent<CarController>();
            CurrentCar.transform.position = Vector3.zero; //TODO: use some spawn point
            CurrentCar.transform.rotation = Quaternion.identity; //TODO: use some spawn point
            CurrentCar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                
            yield return CurrentCar.customisation.ApplyVehicleChanges(defaultCarData);

            CameraController.Instance.SetTarget(CurrentCar.transform);

            onComplete?.Invoke();
        }
    }
}
