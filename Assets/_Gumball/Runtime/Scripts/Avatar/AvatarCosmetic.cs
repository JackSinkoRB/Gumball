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

        [Foldout("Debugging"), SerializeField, ReadOnly] protected Avatar avatarBelongsTo;
        [Foldout("Debugging"), SerializeField, ReadOnly] protected int currentIndex = -1;

        protected string saveKey => $"{avatarBelongsTo.SaveKey}.CosmeticsData.{avatarBelongsTo.CurrentBodyType.ToString()}.{displayName}";

        public AvatarCosmeticCategory Category => category;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int CurrentIndex => currentIndex;

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
