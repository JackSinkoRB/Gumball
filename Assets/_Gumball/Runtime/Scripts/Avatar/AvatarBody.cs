using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CC;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarBody : MonoBehaviour
    {
        
        [SerializeField] private AvatarBodyType bodyType;
        [SerializeField] private CharacterCustomization customiser;
        [SerializeField] private Transform cosmeticsHolder;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar avatarBelongsTo;
        [SerializeField, ReadOnly] private AvatarCosmetic[] cosmetics;
        [SerializeField, ReadOnly] private Material[] attachedMaterialsCached;
        
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private SkinnedMeshRenderer[] skinnedMeshRenderersCached => skinnedMeshRenderers ??= gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        private BlendshapeManager[] blendShapeManagers;
        
        public AvatarBodyType BodyType => bodyType;
        public CharacterCustomization Customiser => customiser;
        public AvatarCosmetic[] Cosmetics => cosmetics;

        public Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> CosmeticsGrouped { get; } = new();

        public Material[] AttachedMaterials
        {
            get
            {
                if (attachedMaterialsCached == null || attachedMaterialsCached.Length == 0)
                {
                    HashSet<Material> attachedMaterials = new();
                    foreach (SkinnedMeshRenderer mesh in skinnedMeshRenderersCached)
                    {
                        foreach (Material material in mesh.materials)
                        {
                            attachedMaterials.Add(material);
                        }
                    }

                    attachedMaterialsCached = attachedMaterials.ToArray();
                }

                return attachedMaterialsCached;
            }
        }
        public void Initialise(Avatar avatar)
        {
            avatarBelongsTo = avatar;

            FindCosmetics();

            foreach (AvatarCosmetic cosmetic in cosmetics)
            {
                cosmetic.Initialise(avatar);
            }

            GroupCosmeticsByCategory();
        }
        
        public T GetCosmetic<T>() where T : AvatarCosmetic
        {
            foreach (AvatarCosmetic cosmetic in cosmetics)
            {
                if (cosmetic.GetType() == typeof(T))
                    return (T) cosmetic;
            }

            throw new NullReferenceException($"There is no cosmetic of type {typeof(T)}");
        }

        public void LoadCosmetics()
        {
            foreach (AvatarCosmetic cosmetic in cosmetics)
            {
                cosmetic.Apply(cosmetic.GetSavedIndex());
            }
        }
        
        /// <summary>
        /// Gets and caches all of the blend shape managers on the avatar body.
        /// </summary>
        /// <returns></returns>
        public BlendshapeManager[] GetBlendShapeManagers()
        {
            if (blendShapeManagers == null || blendShapeManagers.Length == 0)
                InitialiseBlendShapeManagers();

            return blendShapeManagers;
        }
        
        public void SetBlendshape(string propertyName, float value)
        {
            foreach (BlendshapeManager manager in GetBlendShapeManagers())
            {
                manager.SetBlendshape(propertyName, value);
            }
        }

        private void InitialiseBlendShapeManagers()
        {
            HashSet<BlendshapeManager> managers = new();
            
            foreach (SkinnedMeshRenderer mesh in skinnedMeshRenderersCached)
            {
                managers.Add(mesh.gameObject.AddComponent<BlendshapeManager>());
            }

            blendShapeManagers = managers.ToArray();
        }

        private void FindCosmetics()
        {
            HashSet<AvatarCosmetic> cosmeticsFound = new();
            foreach (AvatarCosmetic cosmetic in cosmeticsHolder.GetComponentsInAllChildren<AvatarCosmetic>())
            {
                cosmeticsFound.Add(cosmetic);
            }

            cosmetics = cosmeticsFound.ToArray();
        }
        
        private void GroupCosmeticsByCategory()
        {
            CosmeticsGrouped.Clear();
            
            foreach (AvatarCosmetic cosmetic in cosmetics)
            {
                AvatarCosmeticCategory category = cosmetic.Category;
                List<AvatarCosmetic> avatarCosmeticsForCategory = new List<AvatarCosmetic>();
                if (CosmeticsGrouped.ContainsKey(category))
                    avatarCosmeticsForCategory.AddRange(CosmeticsGrouped[category]);
                
                avatarCosmeticsForCategory.Add(cosmetic);
                CosmeticsGrouped[category] = avatarCosmeticsForCategory;
            }
        }

    }
}
