using System;
using System.Collections;
using System.Collections.Generic;
using MagneticScrollUtils;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class AvatarCosmetic : MonoBehaviour
    {
        
        #region STATIC
        /// <summary>
        /// Generic way to get the Colorable component of the cosmetic, and/or to check if the cosmetic is colorable.
        /// </summary>
        /// <returns>The colorable option, else null if it's not colorable.</returns>
        public static ColorableCosmeticOption? GetColorable(AvatarCosmetic cosmetic)
        {
            return cosmetic switch
            {
                ItemCosmetic itemCosmetic 
                    when itemCosmetic.CurrentItemData.Colorable.IsColorable && itemCosmetic.CurrentItemData.Colorable.Colors.Length > 1 
                    => itemCosmetic.CurrentItemData.Colorable,
                EyesCosmetic eyesCosmetic => eyesCosmetic.Colorable,
                _ => null //return null if not colorable
            };
        }

        /// <summary>
        /// Generic way to apply a color to a cosmetic, in situations where the specific type is not known. 
        /// </summary>
        public static void ApplyColorIndex(AvatarCosmetic cosmetic, int index)
        {
            switch (cosmetic)
            {
                case ItemCosmetic itemCosmetic:
                    itemCosmetic.ApplyColor(index);
                    break;
                case EyesCosmetic eyesCosmetic:
                    eyesCosmetic.ApplyColor(index);
                    break;
            }
        }

        /// <summary>
        /// A generic way to obtain the current color index for a cosmetic, in situations where the specific type is not known. 
        /// </summary>
        /// <param name="cosmetic"></param>
        public static int GetCurrentColorIndex(AvatarCosmetic cosmetic)
        {
            if (GetColorable(cosmetic) == null)
                throw new InvalidOperationException($"{cosmetic.DisplayName} is not colorable.");

            return cosmetic switch
            {
                ItemCosmetic itemCosmetic => itemCosmetic.CurrentColorIndex,
                EyesCosmetic eyesCosmetic => eyesCosmetic.CurrentColorIndex,
                _ => -1 //should not reach here
            };
        }
        #endregion

        [SerializeField, InitializationField] private AvatarCosmeticCategory category;
        [SerializeField, InitializationField] private string displayName = "NOT ASSIGNED";
        [SerializeField, InitializationField] private Sprite icon;
        [SerializeField, InitializationField] protected int defaultIndex;
        [SerializeField] private AvatarCameraController.CameraPositionType cameraPosition;

        [Foldout("Debugging"), SerializeField, ReadOnly] protected Avatar avatarBelongsTo;
        [Foldout("Debugging"), SerializeField, ReadOnly] protected int currentIndex = -1;

        protected string saveKey => $"{avatarBelongsTo.SaveKey}.CosmeticsData.{avatarBelongsTo.CurrentBodyType.ToString()}.{displayName}";

        public AvatarCosmeticCategory Category => category;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int CurrentIndex => currentIndex;
        public AvatarCameraController.CameraPositionType CameraPosition => cameraPosition;

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
        }

        public virtual int GetSavedIndex()
        {
            return DataManager.Avatar.Get(saveKey, defaultIndex);
        }

        public virtual void Save()
        {
            DataManager.Avatar.Set(saveKey, currentIndex);
        }

        public abstract int GetMaxIndex();

        public abstract void OnCreateScrollItem(ScrollItem scrollItem, int index);

        protected abstract void OnApplyCosmetic(int index);
        
    }
}
