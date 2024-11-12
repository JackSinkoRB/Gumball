using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    [RequireComponent(typeof(Button))]
    public class LockableButton : MonoBehaviour
    {

        [SerializeField] private Unlockable unlockable;
        [Tooltip("An object to show when the unlockable is locked.")]
        [SerializeField] private GameObject lockedObject;
        
        private Button button => GetComponent<Button>();

        private void OnEnable()
        {
            Refresh();
            
            ExperienceManager.onLevelChange += OnLevelChange;
        }

        private void OnDisable()
        {
            ExperienceManager.onLevelChange -= OnLevelChange;
        }

        private void OnLevelChange(int previousLevel, int newLevel)
        {
            Refresh();
        }

        private void Refresh()
        {
            button.interactable = unlockable.IsUnlocked;
            lockedObject.gameObject.SetActive(!unlockable.IsUnlocked);
        }
        
    }
}
