using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarBody : MonoBehaviour
    {
        
        [SerializeField] private AvatarBodyType bodyType;
        [SerializeField] private Transform cosmeticsHolder;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar avatarBelongsTo;
        [SerializeField, ReadOnly] private AvatarCosmetic[] cosmetics;
        
        public AvatarBodyType BodyType => bodyType;
        public AvatarCosmetic[] Cosmetics => cosmetics;

        public Dictionary<AvatarCosmeticCategory, List<AvatarCosmetic>> CosmeticsGrouped { get; } = new();

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
