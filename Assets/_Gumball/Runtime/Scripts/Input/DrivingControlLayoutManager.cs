using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlLayoutManager : MonoBehaviour
    {

        [SerializeField] private DrivingControlLayout[] layouts;

        private int currentLayoutIndex
        {
            get => DataManager.Settings.Get("CurrentDrivingLayoutIndex", 0);
            set => DataManager.Settings.Set("CurrentDrivingLayoutIndex", value);
        } 
        
        public DrivingControlLayout CurrentLayout => layouts[currentLayoutIndex];
        
        public void SetCurrentLayout(int index)
        {
            if (currentLayoutIndex == index)
                return; //already current
            
            currentLayoutIndex = index;
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
