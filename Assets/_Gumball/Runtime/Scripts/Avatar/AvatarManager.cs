using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Avatar Manager")]
    public class AvatarManager : SingletonScriptable<AvatarManager>
    {
        
        [SerializeField] private AssetReferenceGameObject avatarPrefab;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar currentAvatar;

        public Avatar CurrentAvatar => currentAvatar;
        
        public IEnumerator SpawnAvatar(Vector3 position, Quaternion rotation)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(avatarPrefab);
            yield return handle;

            currentAvatar = Instantiate(handle.Result, position, rotation).GetComponent<Avatar>();
            currentAvatar.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);

            yield return currentAvatar.SpawnBodyAndCosmetics();
        }
        
    }
}
