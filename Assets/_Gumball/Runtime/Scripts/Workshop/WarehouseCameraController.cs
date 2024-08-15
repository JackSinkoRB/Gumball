using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WarehouseCameraController : RotatingCameraController
    {
        
        protected override void OnEnable()
        {
            base.OnEnable();

            WarehouseManager.Instance.onCurrentCarChanged += OnCurrentCarChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            WarehouseManager.Instance.onCurrentCarChanged -= OnCurrentCarChanged;
        }

        private void OnCurrentCarChanged(AICar newcar)
        {
            SetTarget(newcar.transform, DefaultTargetOffset);
        }
        
    }
}
