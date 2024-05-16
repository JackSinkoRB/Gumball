using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlLayoutSettingSwitcher : MonoBehaviour
    {

        [SerializeField] private TMP_Dropdown dropdown;

        private bool isInitialised;

        private DrivingControlLayoutManager layoutManager => PanelManager.GetPanel<DrivingControlsPanel>().LayoutManager;

        private void OnEnable()
        {
            if (!isInitialised)
                Initialise();
        }

        private void Initialise()
        {
            SetupDropdownOptions();
        }

        private void SetupDropdownOptions()
        {
            dropdown.ClearOptions();

            List<string> options = new();
            foreach (DrivingControlLayout layout in layoutManager.Layouts)
            {
                options.Add(layout.name);
            }
            dropdown.AddOptions(options);
            
            dropdown.SetValueWithoutNotify(layoutManager.CurrentLayoutIndex);
        }
        
        public void OnDropdownValueChange()
        {
            layoutManager.SetCurrentLayout(dropdown.value);
        }
        
    }
}
