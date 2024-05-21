using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class IntroCameraState : CameraState
    {
        
        public override void OnSetCurrent(CameraController controller)
        {
            base.OnSetCurrent(controller);

            target = WarehouseManager.Instance.CurrentCar.IntroCameraTarget;
        }

    }
}
