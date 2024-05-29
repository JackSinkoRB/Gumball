using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class RearViewCameraState : DrivingCameraState
    {
        
        public override void OnSetCurrent(CameraController controller)
        {
            base.OnSetCurrent(controller);
            
            if (WarehouseManager.Instance.CurrentCar.RearViewCameraTarget == null)
                Debug.LogError($"{WarehouseManager.Instance.CurrentCar.name} is missing the rear view camera target.");
            
            otherTarget = WarehouseManager.Instance.CurrentCar.RearViewCameraTarget;
        }
        
    }
}
