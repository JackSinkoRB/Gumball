using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Avatar Manager")]
    public class AvatarManager : SingletonScriptable<AvatarManager>
    {

        public const AvatarBodyType DefaultDriverBodyType = AvatarBodyType.MALE;
        public const AvatarBodyType DefaultCoDriverBodyType = AvatarBodyType.FEMALE;
        
        [SerializeField] private AssetReferenceGameObject avatarPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar driverAvatar;
        [SerializeField, ReadOnly] private Avatar coDriverAvatar;

        public AssetReferenceGameObject AvatarPrefab => avatarPrefab;
        public Avatar DriverAvatar => driverAvatar;
        public Avatar CoDriverAvatar => coDriverAvatar;

        public void HideAvatars(bool hide)
        {
            if (driverAvatar != null)
                driverAvatar.gameObject.SetActive(!hide);
            if (driverAvatar != null)
                coDriverAvatar.gameObject.SetActive(!hide);
        }
        
        public IEnumerator SpawnDriver(Vector3 position, Quaternion rotation)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(avatarPrefab);
            yield return handle;

            driverAvatar = Instantiate(handle.Result, position, rotation).GetComponent<Avatar>();
            driverAvatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            driverAvatar.Initialise(DefaultDriverBodyType);
            driverAvatar.gameObject.name = "DriverAvatar";
            DontDestroyOnLoad(driverAvatar.gameObject);
            
            yield return driverAvatar.SpawnBody(driverAvatar.SavedBodyType);

            driverAvatar.ChangeBodyType(driverAvatar.SavedBodyType);
            
            GlobalLoggers.AvatarLogger.Log($"Driver avatar loading took {stopwatch.Elapsed.ToPrettyString(true)}");
        }
        
        public IEnumerator SpawnCoDriver(Vector3 position, Quaternion rotation)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(avatarPrefab);
            yield return handle;

            coDriverAvatar = Instantiate(handle.Result, position, rotation).GetComponent<Avatar>();
            coDriverAvatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            coDriverAvatar.Initialise(DefaultCoDriverBodyType);
            coDriverAvatar.gameObject.name = "CoDriverAvatar";
            DontDestroyOnLoad(coDriverAvatar.gameObject);
            
            yield return coDriverAvatar.SpawnBody(coDriverAvatar.SavedBodyType);
            
            coDriverAvatar.ChangeBodyType(coDriverAvatar.SavedBodyType);
            
            GlobalLoggers.AvatarLogger.Log($"CoDriver avatar loading took {stopwatch.Elapsed.ToPrettyString(true)}");
        }
        
    }
}
