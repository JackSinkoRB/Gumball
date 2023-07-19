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

        private CarController currentCar;

        public AsyncOperationHandle<GameObject> SpawnCar(Action onComplete = null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(defaultCarData.assetReference);
            handle.Completed += _ =>
            {
                CarController currentVehicle = Instantiate(handle.Result, transform, false).GetComponent<CarController>();
                currentVehicle.transform.position = Vector3.zero; //TODO: use some spawn point
                currentVehicle.transform.rotation = Quaternion.identity; //TODO: use some spawn point
                currentVehicle.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
                
                currentVehicle.customisation.ApplyVehicleChanges(defaultCarData);
                //CameraControllerWorld.activeController?.Setup(vehicleInstance);

                onComplete?.Invoke();
            };

            return handle;
        }

    }
}
