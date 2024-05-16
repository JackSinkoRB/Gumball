using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlLayoutManager : MonoBehaviour
    {

        [SerializeField] private DrivingControlLayout[] layouts;

        public int CurrentLayoutIndex
        {
            get => DataManager.Settings.Get("CurrentDrivingLayoutIndex", 0);
            private set => DataManager.Settings.Set("CurrentDrivingLayoutIndex", value);
        }

        public DrivingControlLayout[] Layouts => layouts;
        public DrivingControlLayout CurrentLayout => layouts[CurrentLayoutIndex];
        
        public void SetCurrentLayout(int index)
        {
            if (CurrentLayoutIndex == index)
                return; //already current
            
            CurrentLayoutIndex = index;
        }

        public void ShowCurrentLayout()
        {
            foreach (var layout in layouts)
            {
                if (layout == CurrentLayout)
                    layout.SetActive();
                else layout.Disable();
            }
        }
    
    }
}
