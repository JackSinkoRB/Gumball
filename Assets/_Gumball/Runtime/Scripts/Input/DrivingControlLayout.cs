using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DrivingControlLayout : MonoBehaviour
    {
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isActive;
        
        protected DrivingControlLayoutManager layoutManager => PanelManager.GetPanel<DrivingControlsPanel>().LayoutManager;
        
        public void SetActive()
        {
            if (isActive)
                return; //already enabled
            
            OnActivate();
        }

        public void SetInactive()
        {
            if (!isActive)
            {
                //already disabled
                gameObject.SetActive(false); //make sure it's disabled
                return;
            }

            OnDeactivate();
        }

        protected virtual void OnActivate()
        {
            isActive = true;
            gameObject.SetActive(true);
        }
        
        protected virtual void OnDeactivate()
        {
            isActive = false;
            gameObject.SetActive(false);
        }
        
    }
}
