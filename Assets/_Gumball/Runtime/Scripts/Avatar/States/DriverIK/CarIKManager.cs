using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Gumball
{
    /// <summary>
    /// Holds the desired positions for the avatar's body parts.
    /// </summary>
    public class CarIKManager : MonoBehaviour
    {

        [SerializeField] private IKPositionsInCar driver;
        [SerializeField] private IKPositionsInCar passenger;

        public IKPositionsInCar Driver => driver;
        public IKPositionsInCar Passenger => passenger;

        [SerializeField, HideInInspector] private Avatar avatar;
        
        private void OnEnable()
        {
            if (avatar != null)
                DestroyImmediate(avatar.gameObject);
        }

#if UNITY_EDITOR
        [ButtonMethod]
        public void SpawnDriverAvatar()
        {
            SpawnAvatarEditMode(true);
        }

        [ButtonMethod]
        public void SpawnPassengerAvatar()
        {
            SpawnAvatarEditMode(false);
        }

        [ButtonMethod]
        public void DeleteTestAvatar()
        {
            if (avatar != null)
                DestroyImmediate(avatar.gameObject);
        }

        private void SpawnAvatarEditMode(bool isDriver)
        {
            DeleteTestAvatar();
            
            //spawn the base
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(AvatarManager.Instance.AvatarPrefab);
            handle.WaitForCompletion();
            
            avatar = Instantiate(handle.Result, Vector3.zero, Quaternion.Euler(Vector3.zero), transform).GetComponent<Avatar>();
            avatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            
            //spawn the body
            AsyncOperationHandle<GameObject> bodyHandle = Addressables.LoadAssetAsync<GameObject>(avatar.MaleBodyReference);
            bodyHandle.WaitForCompletion();

            AvatarBody newBody = Instantiate(bodyHandle.Result, avatar.transform).GetComponent<AvatarBody>();
            newBody.GetComponent<AddressableReleaseOnDestroy>(true).Init(bodyHandle);
            
            avatar.ForceSetBodyType(newBody);
            
            //setup the driving state
            AICar currentCar = transform.GetComponentInAllParents<AICar>();
            AvatarDrivingState avatarDrivingState = avatar.transform.GetComponentsInAllChildren<AvatarDrivingState>()[0];
            avatarDrivingState.SetupEditMode(currentCar, isDriver);
            avatarDrivingState.OnSetCurrent();
        }
#endif
        
    }
}
