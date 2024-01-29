using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarCharacterSelector : SwitchButton
    {

        private void OnEnable()
        {
            AvatarEditor.onSessionStart += OnSessionStart;
        }
        
        private void OnDisable() {
            AvatarEditor.onSessionStart -= OnSessionStart;
        }

        private void OnSessionStart()
        {
            //initialise the button
            SetButtonSelected(AvatarEditor.Instance.CurrentSelectedAvatar == AvatarManager.Instance.DriverAvatar);
        }

        public override void OnClickLeftSwitch()
        {
            base.OnClickLeftSwitch();
            
            AvatarEditor.Instance.SelectAvatar(true);
        }

        public override void OnClickRightSwitch()
        {
            base.OnClickRightSwitch();
            
            AvatarEditor.Instance.SelectAvatar(false);
        }

    }
}
