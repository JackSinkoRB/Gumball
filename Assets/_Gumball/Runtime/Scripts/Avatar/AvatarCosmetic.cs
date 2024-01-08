using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public abstract class AvatarCosmetic : MonoBehaviour
    {
        
        [SerializeField] private int defaultIndex;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] protected Avatar avatarBelongsTo;
        [SerializeField, ReadOnly] protected int currentIndex;
        
        private string dataSaveKey => $"{avatarBelongsTo.SaveKey}.CosmeticsData.{avatarBelongsTo.CurrentBody.BodyType}.{gameObject.name}";

        private int savedIndex
        {
            get => DataManager.Avatar.Get(dataSaveKey, defaultIndex);
            set => DataManager.Avatar.Set(dataSaveKey, value);
        }
        
        public void Initialise(Avatar avatar)
        {
            avatarBelongsTo = avatar;
            
            //load the saved data
            Apply(savedIndex);
        }

        /// <summary>
        /// Apply a certain cosmetic at the specified index to the avatar.
        /// </summary>
        public void Apply(int index)
        {
            currentIndex = index;
            OnApplyCosmetic(index);
        }
        
        public void SaveData()
        {
            savedIndex = currentIndex;
        }

        protected abstract void OnApplyCosmetic(int index);
        
    }
}
