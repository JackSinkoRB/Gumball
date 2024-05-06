using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Parts/Core Part")]
    public class CorePart : ScriptableObject
    {

        public enum PartType
        {
            ENGINE,
            HANDLING
        }

        public enum Rating
        {
            STREET,
            SUPERCAR
        }

        [SerializeField] private PartType type;
        [SerializeField] private Rating rating;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private int uniqueID = -1;
        
        private string saveKey => $"{type.ToString()}-{name}-{uniqueID}";

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        
        public bool IsUnlocked
        {
            get => DataManager.Cars.Get($"Parts.Core.{saveKey}.IsUnlocked", false);
            private set => DataManager.Cars.Set($"Parts.Core.{saveKey}.IsUnlocked", value);
        }

        private void OnValidate()
        {
            if (uniqueID == -1)
                uniqueID = Random.Range(0, int.MaxValue);

            if (displayName.IsNullOrEmpty())
                displayName = name;
        }

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
        }
        
    }
}
