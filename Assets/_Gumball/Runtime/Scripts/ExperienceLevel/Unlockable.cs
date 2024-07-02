using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Unlockable")]
    public class Unlockable : UniqueScriptableObject
    {

        [SerializeField] private bool isUnlockedByDefault;

        public bool IsUnlocked
        {
            get => DataManager.Player.Get($"Unlockable.{name}-{ID}.IsUnlocked", isUnlockedByDefault);
            private set => DataManager.Player.Set($"Unlockable.{name}-{ID}.IsUnlocked", value);
        }

        [ButtonMethod]
        public void Unlock()
        {
            IsUnlocked = true;
        }
        
        [ButtonMethod]
        public void Lock()
        {
            IsUnlocked = false;
        }

    }
}
