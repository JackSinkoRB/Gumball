using System.Collections;
using System.Collections.Generic;
using CC;
using MyBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball
{
    public class Avatar : MonoBehaviour
    {

        /// <summary>
        /// If the body type hasn't been saved, default to this body type.
        /// </summary>
        private const AvatarBodyType defaultBodyType = AvatarBodyType.MALE;
        
        [SerializeField] private AssetReferenceGameObject femaleBodyReference;
        [SerializeField] private AssetReferenceGameObject maleBodyReference;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AvatarBody currentBody;
        [Tooltip("A unique key for the save data relating to this avatar.")]
        [SerializeField, ReadOnly] private string saveKey;
        
        private string currentBodyTypeKey => $"{saveKey}.CurrentBodyType";
        
        private AvatarBodyType savedBodyType
        {
            get => DataManager.Avatar.Get(currentBodyTypeKey, defaultBodyType);
            set => DataManager.Avatar.Set(currentBodyTypeKey, value);
        }

        /// <summary>
        /// A unique key for the save data relating to this avatar.
        /// </summary>
        public string SaveKey => saveKey;
        public AvatarBody CurrentBody => currentBody;

        /// <summary>
        /// Spawns the avatar's body with applied cosmetics using the data from the save data.
        /// </summary>
        public IEnumerator SpawnBody()
        {
            DontDestroyOnLoad(gameObject);
            
            AssetReferenceGameObject currentBodyReference = savedBodyType == AvatarBodyType.MALE ? maleBodyReference : femaleBodyReference;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(currentBodyReference);
            yield return handle;

            currentBody = Instantiate(handle.Result, transform).GetComponent<AvatarBody>();
            currentBody.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);
            currentBody.Initialise(this);
        }
        
    }
}
