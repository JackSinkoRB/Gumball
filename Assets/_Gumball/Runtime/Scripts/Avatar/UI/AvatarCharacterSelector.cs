using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class AvatarCharacterSelector : MonoBehaviour
    {

        [SerializeField] private Button driverButton;
        [SerializeField] private Button coDriverButton;
        [SerializeField] private Color buttonColorSelected;
        [SerializeField] private Color buttonColorUnselected;
        
        public void OnClickDriverSwitch()
        {
            AvatarEditor.Instance.SelectAvatar(true);
            SetButtonSelected(true);
        }

        public void OnClickCoDriverSwitch()
        {
            AvatarEditor.Instance.SelectAvatar(false);
            SetButtonSelected(false);
        }
        
        private void SetButtonSelected(bool driver)
        {
            driverButton.image.color = driver ? buttonColorSelected : buttonColorUnselected;
            coDriverButton.image.color = !driver ? buttonColorSelected : buttonColorUnselected;
        }
        
    }
}
