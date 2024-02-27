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
            
            SetTarget(WarehouseManager.Instance.CurrentCar.transform, defaultTargetOffset);
            SetInitialPosition();
        }
        
    }
}
