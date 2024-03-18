using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [Serializable]
    public abstract class GameSession : ScriptableObject
    {
        
        [SerializeField] private AssetReferenceT<ChunkMap> chunkMapAssetReference;

        public AssetReferenceT<ChunkMap> ChunkMapAssetReference => chunkMapAssetReference;

        public abstract string GetName();

        public void StartSession()
        {
            CoroutineHelper.Instance.StartCoroutine(StartSessionIE());
        }

        public IEnumerator SetupSession()
        {
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            
            //load the map:
            AsyncOperationHandle<ChunkMap> handle = Addressables.LoadAssetAsync<ChunkMap>(chunkMapAssetReference);
            yield return handle;
            
            ChunkMap chunkMap = handle.Result;

            AvatarManager.Instance.HideAvatars(true);
            //freeze the car
            Rigidbody currentCarRigidbody = WarehouseManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.velocity = Vector3.zero;
            currentCarRigidbody.angularVelocity = Vector3.zero;
            currentCarRigidbody.isKinematic = true;
            
            //move the car to the right position
            Vector3 startingPosition = chunkMap.VehicleStartingPosition;
            Vector3 startingRotation = chunkMap.VehicleStartingRotation;
            currentCarRigidbody.Move(startingPosition, Quaternion.Euler(startingRotation));
            GlobalLoggers.LoadingLogger.Log($"Moved vehicle to map's starting position: {startingPosition}");
            
            //load the map chunks
            Stopwatch chunkLoadingStopwatch = Stopwatch.StartNew();
            yield return ChunkManager.Instance.LoadMap(chunkMap);
            chunkLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"Loaded chunks for map in {chunkLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //set car rigidbody as dynamic
            currentCarRigidbody.isKinematic = false;

            //set driving states:
            if (AvatarManager.Instance.DriverAvatar != null && AvatarManager.Instance.CoDriverAvatar != null)
            {
                AvatarManager.Instance.HideAvatars(false);
                AvatarManager.Instance.DriverAvatar.StateManager.SetState<AvatarDrivingState>();
                AvatarManager.Instance.CoDriverAvatar.StateManager.SetState<AvatarDrivingState>();
            }

            GlobalLoggers.LoadingLogger.Log("Loading session...");
            yield return OnSessionLoad();
            GlobalLoggers.LoadingLogger.Log("Loaded session");

            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.Car);
        }
        
        protected virtual IEnumerator OnSessionLoad()
        {
            yield break;
        }

        private IEnumerator StartSessionIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            yield return MapDrivingSceneManager.LoadMapDrivingSceneIE();
            yield return SetupSession();

            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        
    }
}
