using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class UnlockableAnnouncementPanel : AnimatedPanel
    {

        [SerializeField] private TextMeshProUGUI unlockNameLabel;
        
        public void Populate(Unlockable unlockable)
        {
            unlockNameLabel.text = unlockable.name;
        }
        
    }
}
