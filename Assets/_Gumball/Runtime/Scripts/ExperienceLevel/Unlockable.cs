using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// A scriptable object that can be unlocked when a certain level is reached, or unlocked/locked manually.
    /// </summary>
    [CreateAssetMenu(menuName = "Gumball/Unlockable")]
    public class Unlockable : UniqueScriptableObject
    {

        [SerializeField] private bool unlockWithLevel = true;
        [SerializeField, ConditionalField(nameof(unlockWithLevel)), MinValue(1)] private int requiredLevel = 1;
        [SerializeField, ConditionalField(nameof(unlockWithLevel), true)] private bool isUnlockedByDefault;
        
        public bool IsUnlocked
        {
            get
            {
                if (unlockWithLevel)
                    return ExperienceManager.LevelValue >= requiredLevel;
                
                return DataManager.Player.Get($"Unlockable.{name}-{ID}.IsUnlocked", isUnlockedByDefault);
            }
            private set
            {
                if (!unlockWithLevel)
                    DataManager.Player.Set($"Unlockable.{name}-{ID}.IsUnlocked", value);
            }
        }

        [ButtonMethod(ButtonMethodDrawOrder.AfterInspector, nameof(unlockWithLevel), true)]
        public void Unlock()
        {
            IsUnlocked = true;
        }
        
        [ButtonMethod(ButtonMethodDrawOrder.AfterInspector, nameof(unlockWithLevel), true)]
        public void Lock()
        {
            IsUnlocked = false;
        }
        
    }
}
