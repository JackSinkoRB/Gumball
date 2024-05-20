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
                return; //already disabled

            OnDeactivate();
        }

        protected virtual void OnActivate()
        {
            gameObject.SetActive(true);
        }
        
        protected virtual void OnDeactivate()
        {
            gameObject.SetActive(false);
        }
        
    }
}
