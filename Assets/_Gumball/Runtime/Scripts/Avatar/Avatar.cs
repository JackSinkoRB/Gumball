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

        public delegate void OnChangeBodyTypeDelegate(Avatar avatar, AvatarBodyType previousBodyType, AvatarBodyType newBodyType);
        public static event OnChangeBodyTypeDelegate onChangeBodyType;

        private static readonly int MouthMask = Shader.PropertyToID("_MouthMask");

        [SerializeField] private AvatarStateManager stateManager;
        [SerializeField] private AssetReferenceGameObject femaleBodyReference;
        [SerializeField] private AssetReferenceGameObject maleBodyReference;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private AvatarBodyType currentBodyType;
        
        /// <summary>
        /// If the body type hasn't been saved, default to this body type.
        /// </summary>
        private AvatarBodyType defaultBodyType;
        private readonly Dictionary<Material, Shader> defaultShaders = new();

        private string savedBodyTypeKey => $"{SaveKey}.CurrentBodyType";

        public AvatarBody CurrentBody => currentBodyType == AvatarBodyType.MALE ? CurrentMaleBody : CurrentFemaleBody;
        public AvatarStateManager StateManager => stateManager;
        
        public AvatarBodyType SavedBodyType
        {
            get => DataManager.Avatar.Get(savedBodyTypeKey, defaultBodyType);
            set => DataManager.Avatar.Set(savedBodyTypeKey, value);
        }

        /// <summary>
        /// A unique key for the save data relating to this avatar.
        /// </summary>
        public string SaveKey => gameObject.name;
        public AvatarBodyType CurrentBodyType => currentBodyType;

        public AvatarBody CurrentMaleBody { get; private set; }
        public AvatarBody CurrentFemaleBody { get; private set; }

        public AssetReferenceGameObject MaleBodyReference => maleBodyReference;
        
        public void Initialise(AvatarBodyType defaultBodyType)
        {
            this.defaultBodyType = defaultBodyType;
        }
        
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
        
#if UNITY_EDITOR
        public void ForceSetBodyType(AvatarBody body)
        {
            currentBodyType = body.BodyType;
            if (currentBodyType == AvatarBodyType.MALE)
                CurrentMaleBody = body;
            if (currentBodyType == AvatarBodyType.FEMALE)
                CurrentFemaleBody = body;
        }        
#endif

        public void ChangeBodyType(AvatarBodyType newBodyType)
        {
            if (newBodyType == CurrentBodyType)
                return; //already assigned

            //save before changing
            AvatarEditor.SaveCurrentAvatarBody();

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
                cosmetic.Save();
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

        public void EnableHead(bool enable)
        {
            //head cosmetics:
            HairCosmetic hairCosmetic = CurrentBody.GetCosmetic<HairCosmetic>();
            if (hairCosmetic.CurrentItem != null)
                hairCosmetic.CurrentItem.SetActive(enable);
            
            EyewearCosmetic eyewearCosmetic = CurrentBody.GetCosmetic<EyewearCosmetic>();
            if (eyewearCosmetic.CurrentItem != null)
                eyewearCosmetic.CurrentItem.SetActive(enable);
            
            BeardCosmetic beardCosmetic = CurrentBody.GetCosmetic<BeardCosmetic>();
            if (beardCosmetic != null && beardCosmetic.CurrentItem != null)
                beardCosmetic.CurrentItem.SetActive(enable);
            
            //head materials:
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in CurrentBody.SkinnedMeshRenderers)
            {
                foreach (Material material in skinnedMeshRenderer.materials)
                {
                    const string bodyMaterialName = "M_Skin_Body";
                    if (material.name.Contains(bodyMaterialName))
                    {
                        //set the mouth mask
                        material.SetTexture(MouthMask, enable ? null : AvatarManager.Instance.MouthMask);
                        continue;
                    }

                    if (!enable && !defaultShaders.ContainsKey(material))
                        defaultShaders[material] = material.shader;
                    
                    material.shader = enable ? defaultShaders[material] : AvatarManager.Instance.InvisibleShader;
                }
            }
        }
        
    }
}
