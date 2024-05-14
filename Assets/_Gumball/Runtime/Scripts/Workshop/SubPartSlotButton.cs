using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SubPartSlotButton : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image icon;

        public void Initialise(SubPartSlot slot)
        {
            label.text = slot.Type.ToFriendlyString();
            icon.sprite = slot.Icon;
        }

        public void OnClick()
        {
            PanelManager.GetPanel<SwapSubPartPanel>().Show();
        }
        
    }
}
