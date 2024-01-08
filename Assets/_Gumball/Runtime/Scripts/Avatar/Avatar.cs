using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class Avatar : MonoBehaviour
    {

        private const AvatarBodyType defaultBodyType = AvatarBodyType.MALE;

        private const string currentBodyTypeKey = "CurrentBodyType";
        
        [SerializeField] private AssetReferenceGameObject femaleBodyReference;
        [SerializeField] private AssetReferenceGameObject maleBodyReference;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AvatarBodyCosmetics currentBody;
        
        private AvatarBodyType savedBodyType
        {
            get => DataManager.Avatar.Get(currentBodyTypeKey, defaultBodyType);
            set => DataManager.Avatar.Set(currentBodyTypeKey, value);
        }

        public IEnumerator Initialise()
        {
            DontDestroyOnLoad(gameObject);
            
            yield return SpawnBody();
            yield return currentBody.ApplyCosmeticsFromSaveData();
        }
        
        public IEnumerator ChangeBodyType(AvatarBodyType bodyType)
        {
            savedBodyType = bodyType;
            yield return Initialise();
        }

        private IEnumerator SpawnBody()
        {
            AssetReferenceGameObject currentBodyReference = savedBodyType == AvatarBodyType.MALE ? maleBodyReference : femaleBodyReference;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(currentBodyReference);
            yield return handle;

            currentBody = Instantiate(handle.Result, transform).GetComponent<AvatarBodyCosmetics>();
            currentBody.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
        }

    }
}
