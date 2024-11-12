using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DrivingControlLayoutSettingUI : MonoBehaviour
    {

        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI currentLayoutName;
        
        private DrivingControlLayoutManager layoutManager => PanelManager.GetPanel<DrivingControlsPanel>().LayoutManager;

        private void OnEnable()
        {
            Refresh();
        }

        public void OnClickLeftButton()
        {
            if (layoutManager.CurrentLayoutIndex == 0)
                return; //no more layouts

            int newLayoutIndex = layoutManager.CurrentLayoutIndex - 1;
            layoutManager.SetCurrentLayout(newLayoutIndex);
            
            Refresh();
        }

        public void OnClickRightButton()
        {
            if (layoutManager.CurrentLayoutIndex >= layoutManager.Layouts.Length - 1)
                return; //no more layouts
            
            int newLayoutIndex = layoutManager.CurrentLayoutIndex + 1;
            layoutManager.SetCurrentLayout(newLayoutIndex);
            
            Refresh();
        }

        private void Refresh()
        {
            leftButton.interactable = layoutManager.CurrentLayoutIndex > 0;
            rightButton.interactable = layoutManager.CurrentLayoutIndex < layoutManager.Layouts.Length - 1;

            currentLayoutName.text = layoutManager.CurrentLayout.name;
        }
        
    }
}
