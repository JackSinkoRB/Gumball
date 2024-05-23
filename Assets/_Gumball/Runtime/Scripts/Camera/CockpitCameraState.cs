using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CockpitCameraState : DrivingCameraState
    {
        
        public override void OnSetCurrent(CameraController controller)
        {
            base.OnSetCurrent(controller);
            
            otherTarget = WarehouseManager.Instance.CurrentCar.CockpitCameraTarget;
        }

    }
}
