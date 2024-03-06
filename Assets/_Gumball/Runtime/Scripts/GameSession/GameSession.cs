using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public abstract class GameSession : ScriptableObject
    {
        
        [SerializeField] private ChunkMap chunkMap;

        public ChunkMap ChunkMap => chunkMap;

        public abstract string GetName();

        public void StartSession()
        {
            CoroutineHelper.Instance.StartCoroutine(StartSessionIE());
        }

        public IEnumerator SetupSession()
        {
            WarehouseManager.Instance.CurrentCar.gameObject.SetActive(true);
            
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
