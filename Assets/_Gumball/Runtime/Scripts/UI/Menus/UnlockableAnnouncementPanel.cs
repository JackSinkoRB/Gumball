using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class UnlockableAnnouncementPanel : AnimatedPanel
    {

        [SerializeField] private AutosizeTextMeshPro unlockNameLabel;
        [SerializeField] private Image icon;
        
        public void Populate(Unlockable unlockable)
        {
            unlockNameLabel.text = unlockable.name;
            unlockNameLabel.Resize();
            
            icon.sprite = unlockable.Icon;
        }
        
    }
}
