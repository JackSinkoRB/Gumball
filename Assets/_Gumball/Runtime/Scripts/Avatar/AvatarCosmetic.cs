using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class AvatarCosmetic : MonoBehaviour
    {

        [SerializeField, InitializationField] private AvatarCosmeticCategory category;
        [SerializeField, InitializationField] private string displayName = "NOT ASSIGNED";
        [SerializeField, InitializationField] private Sprite icon;
        [SerializeField, InitializationField] protected int defaultIndex;
        
        [Space(5)]
        [SerializeField, InitializationField] private bool isColorable;
        [SerializeField, InitializationField] protected string[] colorMaterialProperties;
        [SerializeField, InitializationField, ConditionalField(nameof(isColorable))] protected int defaultColorIndex;
        [SerializeField, ConditionalField(nameof(isColorable))] private CollectionWrapperColor colors;

        [Foldout("Debugging"), SerializeField, ReadOnly] protected Avatar avatarBelongsTo;
        [Foldout("Debugging"), SerializeField, ReadOnly] protected int currentIndex = -1;
        [Foldout("Debugging"), SerializeField, ReadOnly] protected int currentColorIndex = -1;

        private string saveKey => $"{avatarBelongsTo.SaveKey}.CosmeticsData.{avatarBelongsTo.CurrentBodyType.ToString()}.{displayName}";
        private string colourSaveKey => $"{saveKey}.Colour";

        public AvatarCosmeticCategory Category => category;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public bool IsColorable => isColorable && colors != null && colors.Value != null && colors.Value.Length > 0;
        public Color[] Colors => colors.Value;
        public int CurrentIndex => currentIndex;
        public int CurrentColorIndex => currentColorIndex;

        public virtual void Initialise(Avatar avatar)
        {
            avatarBelongsTo = avatar;
        }

        /// <summary>
        /// Apply a certain cosmetic at the specified index to the avatar.
        /// </summary>
        public void Apply(int index)
        {
            if (currentIndex == index)
                return; //already selected
            
            currentIndex = index;
            OnApplyCosmetic(index);
            
            if (IsColorable)
                ApplyColor(currentColorIndex == -1 ? GetSavedColourIndex() : currentColorIndex);
        }

        public virtual int GetSavedIndex()
        {
            return DataManager.Avatar.Get(saveKey, defaultIndex);
        }
        
        public int GetSavedColourIndex()
        {
            return DataManager.Avatar.Get(colourSaveKey, defaultColorIndex);
        }
        
        public virtual void Save()
        {
            DataManager.Avatar.Set(saveKey, currentIndex);
            DataManager.Avatar.Set(colourSaveKey, currentColorIndex);
        }

        public abstract int GetMaxIndex();

        public int GetMaxColorIndex()
        {
            return Colors.Length - 1;
        }

        public abstract void OnCreateScrollItem(ScrollItem scrollItem, int index);
        
        public void ApplyColor(int index)
        {
            if (currentColorIndex == index)
                return; //already selected
            
            currentColorIndex = index;
            OnApplyColor(colors.Value[currentColorIndex]);
        }
        
        public void OnApplyColor(Color color)
        {
            foreach (Material material in GetMaterialsWithColorProperty())
            {
                foreach (string property in colorMaterialProperties)
                {
                    material.SetColor(property, color);
                }
            }
        }
        
        protected virtual HashSet<Material> GetMaterialsWithColorProperty()
        {
            HashSet<Material> materials = new HashSet<Material>();
            foreach (Material material in avatarBelongsTo.CurrentBody.AttachedMaterials)
            {
                foreach (string property in colorMaterialProperties)
                {
                    if (material.HasProperty(property))
                    {
                        materials.Add(material);
                        break;
                    }
                }
            }
            
            return materials;
        }
        
        protected abstract void OnApplyCosmetic(int index);

    }
}
