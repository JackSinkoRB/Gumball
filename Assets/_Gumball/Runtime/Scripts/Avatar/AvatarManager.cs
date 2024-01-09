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
    [CreateAssetMenu(menuName = "Gumball/Singletons/Avatar Manager")]
    public class AvatarManager : SingletonScriptable<AvatarManager>
    {
        
        [SerializeField] private AssetReferenceGameObject avatarPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar driverAvatar;
        [SerializeField, ReadOnly] private Avatar coDriverAvatar;

        public Avatar DriverAvatar => driverAvatar;
        public Avatar CoDriverAvatar => coDriverAvatar;

        public IEnumerator SpawnDriver(Vector3 position, Quaternion rotation)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(avatarPrefab);
            yield return handle;

            driverAvatar = Instantiate(handle.Result, position, rotation).GetComponent<Avatar>();
            driverAvatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            driverAvatar.gameObject.name = "DriverAvatar";
            
            yield return driverAvatar.SpawnBody();
#if ENABLE_LOGS
            Debug.Log($"Driver avatar loading took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
        
        public IEnumerator SpawnCoDriver(Vector3 position, Quaternion rotation)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(avatarPrefab);
            yield return handle;

            coDriverAvatar = Instantiate(handle.Result, position, rotation).GetComponent<Avatar>();
            coDriverAvatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            coDriverAvatar.gameObject.name = "CoDriverAvatar";

            yield return coDriverAvatar.SpawnBody();
#if ENABLE_LOGS
            Debug.Log($"CoDriver avatar loading took {stopwatch.Elapsed.ToPrettyString(true)}");
#endif
        }
        
    }
}
