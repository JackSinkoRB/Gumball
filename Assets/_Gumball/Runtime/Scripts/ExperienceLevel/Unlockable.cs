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

        [SerializeField] private bool isUnlockedByDefault;
        [Tooltip("Does the unlockable announcement panel get shown when the unlockable is unlocked?")]
        [SerializeField] private bool announceWhenUnlocked = true;
        
        public bool IsUnlocked
        {
            get => DataManager.Player.Get($"Unlockable.{name}-{ID}.IsUnlocked", isUnlockedByDefault);
            private set => DataManager.Player.Set($"Unlockable.{name}-{ID}.IsUnlocked", value);
        }

        [ButtonMethod]
        public void Unlock()
        {
            if (IsUnlocked)
                return;
            
            OnUnlock();
        }
        
        [ButtonMethod]
        public void Lock()
        {
            if (!IsUnlocked)
                return;
            
            OnLock();
        }
        
        private void OnUnlock()
        {
            IsUnlocked = true;

            if (announceWhenUnlocked)
            {
                PanelManager.GetPanel<UnlockableAnnouncementPanel>().Show();
                PanelManager.GetPanel<UnlockableAnnouncementPanel>().Populate(this);
            }
        }

        private void OnLock()
        {
            IsUnlocked = false;
        }
        
    }
}
