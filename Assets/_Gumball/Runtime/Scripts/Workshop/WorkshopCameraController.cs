using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class WorkshopCameraController : RotatingCameraController
    {
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (WarehouseManager.Instance.CurrentCar != null)
                SetTarget(WarehouseManager.Instance.CurrentCar.transform, defaultTargetOffset);
            
            SetInitialPosition();
        }
        
    }
}
