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

        public delegate void OnChangeBodyTypeDelegate(Avatar avatar, AvatarBodyType previousBodyType, AvatarBodyType newBodyType);
        public static event OnChangeBodyTypeDelegate onChangeBodyType;
        
        /// <summary>
        /// If the body type hasn't been saved, default to this body type.
        /// </summary>
        public const AvatarBodyType DefaultBodyType = AvatarBodyType.MALE;
        
        [SerializeField] private AssetReferenceGameObject femaleBodyReference;
        [SerializeField] private AssetReferenceGameObject maleBodyReference;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AvatarBodyType currentBodyType;
        
        private string savedBodyTypeKey => $"{SaveKey}.CurrentBodyType";

        public AvatarBody CurrentBody => currentBodyType == AvatarBodyType.MALE ? CurrentMaleBody : CurrentFemaleBody;
        
        public AvatarBodyType SavedBodyType
        {
            get => DataManager.Avatar.Get(savedBodyTypeKey, DefaultBodyType);
            set => DataManager.Avatar.Set(savedBodyTypeKey, value);
        }
        
        /// <summary>
        /// A unique key for the save data relating to this avatar.
        /// </summary>
        public string SaveKey => gameObject.name;
        public AvatarBodyType CurrentBodyType => currentBodyType;

        public AvatarBody CurrentMaleBody { get; private set; }
        public AvatarBody CurrentFemaleBody { get; private set; }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        /// <summary>
        /// Spawns the avatar body with applied cosmetics.
        /// </summary>
        public IEnumerator SpawnBody(AvatarBodyType bodyType)
        {
            AssetReferenceGameObject currentBodyReference = bodyType == AvatarBodyType.MALE ? maleBodyReference : femaleBodyReference;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(currentBodyReference);
            yield return handle;

            AvatarBody newBody = Instantiate(handle.Result, transform).GetComponent<AvatarBody>();
            newBody.GetComponent<AddressableReleaseOnDestroy>(true).Init(handle);

            //assign to specific body
            if (bodyType == AvatarBodyType.FEMALE)
                CurrentFemaleBody = newBody;
            if (bodyType == AvatarBodyType.MALE)
                CurrentMaleBody = newBody;
            
            newBody.Initialise(this);
        }

        public void ChangeBodyType(AvatarBodyType newBodyType)
        {
            if (newBodyType == CurrentBodyType)
                return; //already assigned

            AvatarBody previousBody = currentBodyType == AvatarBodyType.MALE ? CurrentMaleBody : CurrentFemaleBody;
            currentBodyType = newBodyType;
            
            if (previousBody != null)
                previousBody.gameObject.SetActive(false);
            
            AvatarBody newBody = newBodyType == AvatarBodyType.MALE ? CurrentMaleBody : CurrentFemaleBody;
            newBody.LoadCosmetics();
            newBody.gameObject.SetActive(true);

            onChangeBodyType?.Invoke(this, previousBody == null ? AvatarBodyType.NONE : previousBody.BodyType, newBodyType);
        }

        public void SaveBodyCosmetics(AvatarBody body)
        {
            foreach (AvatarCosmetic cosmetic in body.Cosmetics)
            {
                cosmetic.SaveIndex();
            }
        }
        
        /// <summary>
        /// Loads and creates all the body types. Useful in the avatar editor so switching between them is seamless.
        /// </summary>
        public IEnumerator EnsureAllBodiesExist()
        {
            TrackedCoroutine femaleLoading = null;
            if (CurrentFemaleBody == null)
                femaleLoading = new TrackedCoroutine(SpawnBody(AvatarBodyType.FEMALE));
            
            TrackedCoroutine maleLoading = null;
            if (CurrentMaleBody == null)
                maleLoading = new TrackedCoroutine(SpawnBody(AvatarBodyType.MALE));

            yield return new WaitUntil(() => (femaleLoading == null || !femaleLoading.IsPlaying) && (maleLoading == null || !maleLoading.IsPlaying));
            
            //disable the extra bodies
            if (femaleLoading != null)
                CurrentFemaleBody.gameObject.SetActive(false);
            if (maleLoading != null)
                CurrentMaleBody.gameObject.SetActive(false);
        }
        
        public void DestroyAllBodyTypesExceptCurrent()
        {
            AvatarBody bodyToDestroy = currentBodyType == AvatarBodyType.MALE ? CurrentFemaleBody : CurrentMaleBody;
            if (bodyToDestroy == null)
                return; //already destroyed

            //set reference null immediately, as it won't be destroyed until end of frame
            if (currentBodyType == AvatarBodyType.MALE)
                CurrentFemaleBody = null;
            if (currentBodyType == AvatarBodyType.FEMALE)
                CurrentMaleBody = null;
            
            Destroy(bodyToDestroy.gameObject);
        }
        
    }
}
