using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarBodyCosmetics : MonoBehaviour
    {
        
        [SerializeField] private AvatarBodyType bodyType;
        [SerializeField] private AvatarCosmeticsData defaultData;
        
        [Header("Debugging")]
        [Tooltip("The current data that may be dirty. Requires SaveData() to save changes in the data manager.")]
        [SerializeField, ReadOnly] private AvatarCosmeticsData currentUnsavedData;

        private string dataSaveKey => $"CosmeticsData.{bodyType.ToString()}";
        
        public AvatarBodyType BodyType => bodyType;

        public AvatarCosmeticsData CurrentSavedData
        {
            get => DataManager.Avatar.Get(dataSaveKey, defaultData);
            private set => DataManager.Avatar.Set(dataSaveKey, value);
        }

        public IEnumerator ApplyCosmetics(AvatarCosmeticsData data)
        {
            currentUnsavedData = data;
            
            //TODO
            
            yield break;
        }
        
        /// <summary>
        /// Applies the cosmetics using the current save data.
        /// </summary>
        public IEnumerator ApplyCosmeticsFromSaveData()
        {
            yield return ApplyCosmetics(CurrentSavedData);
        }

        /// <summary>
        /// Write changes to the data manager. 
        /// </summary>
        public void SaveData()
        {
            CurrentSavedData = currentUnsavedData;
        }

    }
}
